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

    // Search for lua.h in the locations a C:\lua install might place it:
    //   C:\lua\include\  (pre-built dev package layout)
    //   C:\lua\          (headers copied directly)
    //   C:\lua\src\      (Lua source tarball extracted into C:\lua)
    let include_dir = [
        lua_dir.join("include"),
        lua_dir.clone(),
        lua_dir.join("src"),
    ]
    .into_iter()
    .find(|p| p.join("lua.h").exists())
    .unwrap_or_else(|| {
        panic!(
            "lua.h not found in C:\\lua, C:\\lua\\include, or C:\\lua\\src. \
             Copy the Lua source files (lua.h, luaconf.h, lauxlib.h, lualib.h) \
             into one of those directories."
        )
    });

    // Find whichever lua*.dll is present (e.g. lua54.dll, lua55.dll)
    let lua_dll = find_file_by_prefix(&lua_dir, "lua", "dll").unwrap_or_else(|| {
        panic!("No lua*.dll found in {}", lua_dir.display())
    });
    let link_name = lua_dll.file_stem().unwrap().to_string_lossy().into_owned();

    // Prefer GCC/MinGW: it can link directly against the .dll with no .lib needed.
    let gcc_ok = std::process::Command::new("gcc")
        .args([
            "-shared",
            "-o", output_dll.to_str().unwrap(),
            &format!("-I{}", include_dir.display()),
            c_src.to_str().unwrap(),
            &format!("-L{}", lua_dir.display()),
            &format!("-l{}", link_name),
        ])
        .status()
        .map(|s| s.success())
        .unwrap_or(false);

    if gcc_ok {
        return;
    }

    // Fall back to MSVC. It needs an import .lib, so we generate one on-the-fly
    // from the DLL's exports using dumpbin + lib (both ship with MSVC Build Tools).
    let compiler = cc::Build::new().get_compiler();
    let import_lib = generate_msvc_import_lib(&compiler, &lua_dll, &output_dir);

    let status = compiler
        .to_command()
        .current_dir(&output_dir)
        .args([
            "/nologo",
            "/LD",
            &format!("/I{}", include_dir.display()),
            c_src.to_str().unwrap(),
            "/FeprintTableC.dll",
            import_lib.to_str().unwrap(),
        ])
        .status()
        .expect("Failed to run cl.exe for printTableC.dll");

    assert!(status.success(), "printTableC.dll compilation failed");
}

/// Generates a MSVC import lib (.lib) from a DLL by reading its exports with
/// dumpbin, writing a .def file, and running lib.exe — no pre-existing .lib needed.
fn generate_msvc_import_lib(compiler: &cc::Tool, dll_path: &Path, out_dir: &Path) -> PathBuf {
    let dll_stem = dll_path.file_stem().unwrap().to_string_lossy();
    let lib_path = out_dir.join(format!("{}.lib", dll_stem));
    // Do not cache: always regenerate so a stale lib from a previous failed
    // build never silently produces unresolved-symbol errors.

    // dumpbin and lib.exe live in the same MSVC toolchain directory as cl.exe.
    // Apply the cc-provided environment so they appear on PATH.
    let env: Vec<_> = compiler.env().to_vec();

    let dumpbin_out = {
        let mut cmd = std::process::Command::new("dumpbin");
        for (k, v) in &env {
            cmd.env(k, v);
        }
        cmd.args(["/EXPORTS", "/NOLOGO", dll_path.to_str().unwrap()])
            .output()
            .expect("dumpbin not found — ensure MSVC Build Tools are installed")
    };
    assert!(
        dumpbin_out.status.success(),
        "dumpbin /EXPORTS failed for {}",
        dll_path.display()
    );

    // Pass the full filename (e.g. "lua54.dll") so the LIBRARY directive in the
    // .def file exactly matches the DLL name the linker will look for at runtime.
    let dll_filename = dll_path.file_name().unwrap().to_string_lossy();
    let def_content = dumpbin_to_def(&dll_filename, &String::from_utf8_lossy(&dumpbin_out.stdout));
    let def_path = out_dir.join(format!("{}.def", dll_stem));
    fs::write(&def_path, def_content).expect("Failed to write .def file");

    let lib_status = {
        let mut cmd = std::process::Command::new("lib");
        for (k, v) in &env {
            cmd.env(k, v);
        }
        cmd.args([
            "/NOLOGO",
            "/MACHINE:X64",
            &format!("/DEF:{}", def_path.display()),
            &format!("/OUT:{}", lib_path.display()),
        ])
        .status()
        .expect("lib.exe not found — ensure MSVC Build Tools are installed")
    };
    assert!(
        lib_status.success(),
        "lib.exe failed to generate import lib from {}",
        dll_path.display()
    );

    lib_path
}

/// Parses `dumpbin /EXPORTS` output into a MSVC .def file.
///
/// dumpbin output looks like:
///
///     ordinal hint RVA      name
///                                   ← blank line here, before any data
///           1    0 000053B0 lua_absindex
///           2    1 0000D7F0 lua_arith
///           ...
///                                   ← blank line after last entry
///   Summary
fn dumpbin_to_def(dll_filename: &str, output: &str) -> String {
    let mut def = format!("LIBRARY {}\nEXPORTS\n", dll_filename);
    let mut in_exports = false;
    let mut saw_data = false;

    for line in output.lines() {
        let trimmed = line.trim();

        if !in_exports {
            // Header line: "ordinal hint RVA name"
            if trimmed.starts_with("ordinal") && trimmed.contains("name") {
                in_exports = true;
            }
            continue;
        }

        if trimmed.is_empty() {
            // There is always one blank line between the header and the first entry.
            // Only treat a blank line as end-of-table once we have seen real data.
            if saw_data {
                break;
            }
            continue;
        }

        // Each export line: ordinal  hint  RVA  name  [= forwarded]
        let cols: Vec<&str> = trimmed.split_whitespace().collect();
        if cols.len() >= 4 && !cols[3].contains('=') {
            def.push_str(cols[3]);
            def.push('\n');
            saw_data = true;
        }
    }

    def
}

/// Finds the first file in `dir` whose lowercase name starts with `prefix` and ends with `.ext`.
fn find_file_by_prefix(dir: &Path, prefix: &str, ext: &str) -> Option<PathBuf> {
    let entries = fs::read_dir(dir).ok()?;
    let mut matches: Vec<PathBuf> = entries
        .flatten()
        .filter_map(|e| {
            let name = e.file_name();
            let lower = name.to_string_lossy().to_ascii_lowercase();
            if lower.starts_with(prefix) && lower.ends_with(&format!(".{}", ext)) {
                Some(e.path())
            } else {
                None
            }
        })
        .collect();
    // Sort so results are deterministic; pick the last (highest version lexicographically)
    matches.sort();
    matches.pop()
}

// ── Unix: compile printTableC.c → LuaApp/lua_modules/printTableC.so ──

fn compile_unix(workspace: &Path, c_src: &Path, target_os: &str) {
    let output_dir = workspace.join("LuaApp/lua_modules");
    fs::create_dir_all(&output_dir).expect("Failed to create LuaApp/lua_modules");
    let output_so = output_dir.join("printTableC.so");

    // Check for vendored headers first so the build works without a system Lua dev package.
    let vendored = workspace.join("LuaApp/lua_src");
    let lua_cflags = if vendored.join("lua.h").exists() {
        vec![format!("-I{}", vendored.display())]
    } else {
        lua_include_flags()
    };
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

/// Returns -I flags for Lua headers.
/// Tries pkg-config across all Lua 5.x minor versions (present and future),
/// then falls back to scanning common filesystem paths.
fn lua_include_flags() -> Vec<String> {
    // Try pkg-config with versioned and generic names.
    // Generate names for Lua 5.4 through 5.9 so future minor releases are covered.
    let mut pkg_names: Vec<String> = Vec::new();
    for minor in (4..=9).rev() {
        pkg_names.push(format!("lua5.{}", minor));
        pkg_names.push(format!("lua-5.{}", minor));
        pkg_names.push(format!("lua5{}", minor));
    }
    pkg_names.extend(["lua5".into(), "lua".into()]);

    for name in &pkg_names {
        if let Ok(out) = std::process::Command::new("pkg-config")
            .args(["--cflags", name])
            .output()
        {
            if out.status.success() {
                let s = String::from_utf8_lossy(&out.stdout);
                let flags: Vec<String> = s.split_whitespace().map(str::to_string).collect();
                if !flags.is_empty() {
                    return flags;
                }
            }
        }
    }

    // Fallback: scan common include roots for any lua* subdirectory containing lua.h
    let search_roots = [
        "/usr/include",
        "/usr/local/include",
        "/opt/local/include",       // MacPorts
        "/opt/homebrew/include",    // Homebrew (Apple Silicon)
        "/usr/local/opt/lua/include", // Homebrew (Intel)
    ];

    for root in &search_roots {
        let root_path = Path::new(root);

        // lua.h directly in the root (e.g. /usr/local/include/lua.h)
        if root_path.join("lua.h").exists() {
            return vec![format!("-I{}", root)];
        }

        // Any lua* subdirectory containing lua.h (e.g. /usr/include/lua5.4/, /usr/include/lua/)
        if let Ok(entries) = fs::read_dir(root_path) {
            let mut candidates: Vec<PathBuf> = entries
                .flatten()
                .filter(|e| {
                    e.file_type().map(|t| t.is_dir()).unwrap_or(false)
                        && e.file_name()
                            .to_string_lossy()
                            .to_ascii_lowercase()
                            .starts_with("lua")
                        && e.path().join("lua.h").exists()
                })
                .map(|e| e.path())
                .collect();
            // Sort and prefer the lexicographically last (typically newest version)
            candidates.sort();
            if let Some(dir) = candidates.pop() {
                return vec![format!("-I{}", dir.display())];
            }
        }
    }

    // Last resort: hope lua.h is on the compiler's default search path
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
