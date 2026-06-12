use std::env;
use std::fs;
use std::path::{Path, PathBuf};

fn main() {
    tauri_build::build();

    let target_os = env::var("CARGO_CFG_TARGET_OS").unwrap_or_default();
    let manifest_dir = PathBuf::from(env::var("CARGO_MANIFEST_DIR").unwrap());
    let workspace_dir = manifest_dir.parent().unwrap().to_path_buf();

    // OUT_DIR = target/{profile}/build/{pkg}/out — go up 3 levels to reach target/{profile}/
    let out_dir = PathBuf::from(env::var("OUT_DIR").unwrap());
    let bin_dir = out_dir
        .ancestors()
        .nth(3)
        .expect("Could not determine binary output directory from OUT_DIR")
        .to_path_buf();

    let c_src = workspace_dir.join("LuaApp/lua_src/printTableC.c");

    println!("cargo:rerun-if-changed={}", c_src.display());
    println!(
        "cargo:rerun-if-changed={}",
        workspace_dir.join("push.lua").display()
    );
    println!(
        "cargo:rerun-if-changed={}",
        workspace_dir.join("LuaApp").display()
    );

    if target_os == "windows" {
        compile_windows(&workspace_dir, &c_src);
        bundle_lua_exe(&bin_dir);
    } else {
        compile_unix(&workspace_dir, &c_src, &target_os);
    }

    copy_to_output(&workspace_dir, &bin_dir);
}

// ── Windows: compile printTableC.c → LuaApp/lua_modules/Release/printTableC.dll ──

fn compile_windows(workspace: &Path, c_src: &Path) {
    let output_dir = workspace.join("LuaApp/lua_modules/Release");
    fs::create_dir_all(&output_dir).expect("Failed to create LuaApp/lua_modules/Release");
    let output_dll = output_dir.join("printTableC.dll");

    let lua_dir = PathBuf::from(r"C:\lua");

    // Headers may be directly in C:\lua or in C:\lua\include
    let include_dir = if lua_dir.join("include").join("lua.h").exists() {
        lua_dir.join("include")
    } else {
        lua_dir.clone()
    };

    // Use cc crate to locate and invoke the system C compiler with the right environment
    // (cc handles finding MSVC via the registry, setting up env vars, etc.)
    let compiler = cc::Build::new().get_compiler();

    let status = if compiler.is_like_msvc() {
        // MSVC: /LD creates a DLL; run from output_dir so .obj/.exp/.lib land there
        let lib_path = lua_dir.join("lua54.lib");
        assert!(
            lib_path.exists(),
            "lua54.lib not found at {} — needed for MSVC DLL link",
            lib_path.display()
        );
        compiler
            .to_command()
            .current_dir(&output_dir)
            .args([
                "/nologo",
                "/LD",
                &format!("/I{}", include_dir.display()),
                c_src.to_str().unwrap(),
                "/FeprintTableC.dll",
                lib_path.to_str().unwrap(),
            ])
            .status()
            .expect("Failed to run cl.exe for printTableC.dll")
    } else {
        // MinGW / GCC — links directly against lua54.dll
        compiler
            .to_command()
            .args([
                "-shared",
                "-o",
                output_dll.to_str().unwrap(),
                &format!("-I{}", include_dir.display()),
                c_src.to_str().unwrap(),
                &format!("-L{}", lua_dir.display()),
                "-llua54",
            ])
            .status()
            .expect("Failed to run gcc for printTableC.dll")
    };

    assert!(status.success(), "printTableC.dll compilation failed");
}

// ── Unix: compile printTableC.c → LuaApp/lua_modules/printTableC.so ──

fn compile_unix(workspace: &Path, c_src: &Path, target_os: &str) {
    let output_dir = workspace.join("LuaApp/lua_modules");
    fs::create_dir_all(&output_dir).expect("Failed to create LuaApp/lua_modules");
    let output_so = output_dir.join("printTableC.so");

    let lua_cflags = lua_include_flags();

    let compiler = cc::Build::new().get_compiler();

    let mut args: Vec<String> = Vec::new();

    if target_os == "macos" {
        // macOS Lua modules: -bundle with undefined symbols resolved at load time
        args.push("-bundle".into());
        args.push("-undefined".into());
        args.push("dynamic_lookup".into());
    } else {
        // Linux / other Unix
        args.push("-shared".into());
        args.push("-fPIC".into());
    }

    args.push("-o".into());
    args.push(output_so.to_str().unwrap().into());
    args.extend(lua_cflags);
    args.push(c_src.to_str().unwrap().into());

    let status = compiler
        .to_command()
        .args(&args)
        .status()
        .expect("Failed to run C compiler for printTableC.so");

    assert!(status.success(), "printTableC.so compilation failed");
}

/// Returns -I flags for Lua headers via pkg-config, falling back to common paths.
fn lua_include_flags() -> Vec<String> {
    for name in &["lua5.4", "lua-5.4", "lua54", "lua"] {
        if let Ok(out) = std::process::Command::new("pkg-config")
            .args(["--cflags", name])
            .output()
        {
            if out.status.success() {
                let s = String::from_utf8_lossy(&out.stdout);
                return s.split_whitespace().map(|s| s.to_string()).collect();
            }
        }
    }
    // Fallback: common Lua header locations
    for path in &[
        "/usr/include/lua5.4",
        "/usr/local/include/lua5.4",
        "/usr/include/lua",
        "/usr/local/include",
        "/usr/include",
    ] {
        if Path::new(path).join("lua.h").exists() {
            return vec![format!("-I{}", path)];
        }
    }
    // Last resort: hope it's on the default include path
    vec![]
}

// ── Windows: copy all .exe and .dll files from C:\lua → <bin>/lua/ (dev runs) ──
//
// For production bundles, tauri.windows.conf.json references C:\lua directly,
// so no workspace staging is needed here.

fn bundle_lua_exe(bin_dir: &Path) {
    let lua_src = Path::new(r"C:\lua");
    if !lua_src.exists() {
        panic!("C:\\lua not found — Lua EXE and DLL are required for Windows builds");
    }

    // Copy into the binary output dir so dev/debug runs find the bundled lua
    let bin_lua = bin_dir.join("lua");
    fs::create_dir_all(&bin_lua).expect("Failed to create bin lua/ dir");

    for entry in fs::read_dir(lua_src).expect("Failed to read C:\\lua") {
        let entry = entry.unwrap();
        let path = entry.path();
        let ext = path
            .extension()
            .and_then(|e| e.to_str())
            .map(|e| e.to_ascii_lowercase())
            .unwrap_or_default();

        if ext == "exe" || ext == "dll" {
            let fname = entry.file_name();
            fs::copy(&path, bin_lua.join(&fname))
                .unwrap_or_else(|e| panic!("Failed to copy {} to bin: {}", path.display(), e));
        }
    }
}

// ── Both platforms: copy push.lua and LuaApp/ into the binary output directory ──

fn copy_to_output(workspace: &Path, bin_dir: &Path) {
    // push.lua
    let push_src = workspace.join("push.lua");
    if push_src.exists() {
        fs::copy(&push_src, bin_dir.join("push.lua")).expect("Failed to copy push.lua to bin dir");
    }

    // LuaApp/ (recursive) — compile step has already placed the C module inside here
    let lua_app_src = workspace.join("LuaApp");
    let lua_app_dst = bin_dir.join("LuaApp");
    copy_dir_all(&lua_app_src, &lua_app_dst).expect("Failed to copy LuaApp/ to bin dir");
}

fn copy_dir_all(src: &Path, dst: &Path) -> std::io::Result<()> {
    fs::create_dir_all(dst)?;
    for entry in fs::read_dir(src)? {
        let entry = entry?;
        let dst_path = dst.join(entry.file_name());
        if entry.file_type()?.is_dir() {
            copy_dir_all(&entry.path(), &dst_path)?;
        } else {
            fs::copy(entry.path(), &dst_path)?;
        }
    }
    Ok(())
}
