use chrono::Local;
use serde::{Deserialize, Serialize};
use std::fs;
use std::io::Write;
use std::path::{Path, PathBuf};
use tauri::AppHandle;
use tauri::Manager;

// ─── Data types ──────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct ConfigCharacter {
    pub character_name: String,
    pub realm_name: String,
    pub account: String,
    pub faction: String,
    pub enabled: bool,
}

#[derive(Debug, Serialize, Deserialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct AppConfig {
    pub wtf_path: String,
    pub characters: Vec<ConfigCharacter>,
}

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct CharacterEntry {
    pub name: String,
    pub realm: String,
}

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct AccountInfo {
    pub name: String,
    pub characters: Vec<CharacterEntry>,
}

// ─── Path helpers ─────────────────────────────────────────────────────────────

fn app_data_dir(app: &AppHandle) -> Result<PathBuf, String> {
    app.path().app_data_dir().map_err(|e| e.to_string())
}

fn config_path(app: &AppHandle) -> Result<PathBuf, String> {
    Ok(app_data_dir(app)?.join("config.json"))
}

fn log_path(app: &AppHandle) -> Result<PathBuf, String> {
    Ok(app_data_dir(app)?.join("log.txt"))
}

fn ensure_parent(path: &Path) -> Result<(), String> {
    if let Some(parent) = path.parent() {
        fs::create_dir_all(parent).map_err(|e| e.to_string())?;
    }
    Ok(())
}

// ─── Commands ────────────────────────────────────────────────────────────────

#[tauri::command]
pub fn scan_wtf_folder(wtf_path: String) -> Result<Vec<AccountInfo>, String> {
    let account_base = Path::new(&wtf_path).join("Account");
    if !account_base.is_dir() {
        return Err(format!(
            "'{}' does not contain an 'Account' subdirectory.",
            wtf_path
        ));
    }

    let mut accounts: Vec<AccountInfo> = Vec::new();

    let entries = fs::read_dir(&account_base).map_err(|e| e.to_string())?;
    for entry in entries {
        let entry = entry.map_err(|e| e.to_string())?;
        let path = entry.path();
        if !path.is_dir() {
            continue;
        }

        let name = entry.file_name().to_string_lossy().to_string();
        if !is_account_dir(&name) {
            continue;
        }

        let mut characters: Vec<CharacterEntry> = Vec::new();

        // Each non-SavedVariables subdir of the account dir is a realm.
        let realm_entries = fs::read_dir(&path).map_err(|e| e.to_string())?;
        for realm_entry in realm_entries {
            let realm_entry = realm_entry.map_err(|e| e.to_string())?;
            let realm_path = realm_entry.path();
            if !realm_path.is_dir() {
                continue;
            }
            let realm_name = realm_entry.file_name().to_string_lossy().to_string();
            if realm_name == "SavedVariables" {
                continue;
            }

            // Each subdir of the realm dir is a character.
            let char_entries = fs::read_dir(&realm_path).map_err(|e| e.to_string())?;
            for char_entry in char_entries {
                let char_entry = char_entry.map_err(|e| e.to_string())?;
                if char_entry.path().is_dir() {
                    characters.push(CharacterEntry {
                        name: char_entry.file_name().to_string_lossy().to_string(),
                        realm: realm_name.clone(),
                    });
                }
            }
        }

        accounts.push(AccountInfo { name, characters });
    }

    accounts.sort_by(|a, b| a.name.cmp(&b.name));
    Ok(accounts)
}

fn is_account_dir(name: &str) -> bool {
    let mut parts = name.splitn(2, '#');
    let before = parts.next().unwrap_or("");
    let after = parts.next().unwrap_or("");
    before.len() == 8
        && before.chars().all(|c| c.is_ascii_digit())
        && !after.is_empty()
        && after.chars().all(|c| c.is_ascii_digit())
}

#[tauri::command]
pub fn get_config(app: AppHandle) -> Result<AppConfig, String> {
    let path = config_path(&app)?;
    if !path.exists() {
        return Ok(AppConfig {
            wtf_path: String::new(),
            characters: vec![],
        });
    }
    let content = fs::read_to_string(&path).map_err(|e| e.to_string())?;
    serde_json::from_str(&content).map_err(|e| e.to_string())
}

#[tauri::command]
pub fn save_config(app: AppHandle, config: AppConfig) -> Result<(), String> {
    let path = config_path(&app)?;
    ensure_parent(&path)?;
    let content = serde_json::to_string_pretty(&config).map_err(|e| e.to_string())?;
    fs::write(&path, content).map_err(|e| e.to_string())
}

#[tauri::command]
pub fn get_config_path(app: AppHandle) -> Result<String, String> {
    config_path(&app).map(|p| p.to_string_lossy().to_string())
}

#[tauri::command]
pub fn run_push(
    app: AppHandle,
    addon: String,
    character: String,
) -> Result<String, String> {
    let cfg_path = config_path(&app)?;
    let cfg_path_str = cfg_path.to_string_lossy().to_string();

    // Find push.lua alongside the executable (production) or via resource dir (dev).
    let push_lua = find_push_lua(&app)?;
    let lua_exe = find_lua_exe()?;

    let output = std::process::Command::new(&lua_exe)
        .arg(&push_lua)
        .arg(&addon)
        .arg(&cfg_path_str)
        .arg(&character)
        .output()
        .map_err(|e| format!("Failed to launch Lua: {}", e))?;

    if !output.status.success() {
        let stderr = String::from_utf8_lossy(&output.stderr).to_string();
        let stdout = String::from_utf8_lossy(&output.stdout).to_string();
        return Err(if !stderr.is_empty() { stderr } else { stdout });
    }

    Ok(String::from_utf8_lossy(&output.stdout).to_string())
}

fn find_push_lua(app: &AppHandle) -> Result<PathBuf, String> {
    // In production the resource dir holds push.lua next to the exe.
    if let Ok(resource_dir) = app.path().resource_dir() {
        let candidate = resource_dir.join("push.lua");
        if candidate.exists() {
            return Ok(candidate);
        }
    }
    // Dev fallback: look relative to the cargo manifest directory.
    if let Ok(exe) = std::env::current_exe() {
        // Walk up from target/debug/ to find push.lua in the workspace root.
        let mut dir = exe.parent().map(|p| p.to_path_buf());
        while let Some(d) = dir {
            let candidate = d.join("push.lua");
            if candidate.exists() {
                return Ok(candidate);
            }
            dir = d.parent().map(|p| p.to_path_buf());
        }
    }
    Err("Could not find push.lua. Make sure it is located next to the executable.".to_string())
}

fn find_lua_exe() -> Result<String, String> {
    // On Windows, scan the bundled lua/ directory next to the executable for any lua*.exe.
    // This works regardless of which Lua 5.x minor version is installed.
    #[cfg(target_os = "windows")]
    if let Ok(exe) = std::env::current_exe() {
        if let Some(exe_dir) = exe.parent() {
            if let Some(path) = find_lua_exe_in_dir(&exe_dir.join("lua")) {
                return Ok(path);
            }
        }
    }

    // Fall back to the system PATH. Try the generic "lua" name first (most package
    // managers provide it as a symlink to the installed version), then versioned names
    // covering Lua 5.4 through 5.9 so future minor releases are picked up automatically.
    let mut candidates: Vec<String> = vec!["lua".to_string()];
    for minor in (4..=9).rev() {
        candidates.push(format!("lua5.{}", minor));
        candidates.push(format!("lua5{}", minor));
        candidates.push(format!("lua-5.{}", minor));
    }

    for candidate in &candidates {
        if std::process::Command::new(candidate)
            .arg("-v")
            .output()
            .is_ok()
        {
            return Ok(candidate.clone());
        }
    }

    Err("Lua not found. Please install Lua (5.4 or later) and make sure it is in your PATH."
        .to_string())
}

/// Scans `dir` for the first executable whose name starts with "lua" (case-insensitive).
/// Used on Windows to find whichever lua*.exe was bundled, without hardcoding a version.
#[cfg(target_os = "windows")]
fn find_lua_exe_in_dir(dir: &std::path::Path) -> Option<String> {
    let entries = std::fs::read_dir(dir).ok()?;
    let mut candidates: Vec<std::path::PathBuf> = entries
        .flatten()
        .filter(|e| {
            let name = e.file_name();
            let lower = name.to_string_lossy().to_ascii_lowercase();
            lower.starts_with("lua") && lower.ends_with(".exe")
        })
        .map(|e| e.path())
        .collect();
    // Sort so the result is deterministic; prefer the lexicographically last name
    // (e.g. lua55.exe > lua54.exe, so we pick the newer one if both are present)
    candidates.sort();
    candidates.pop().map(|p| p.to_string_lossy().into_owned())
}

#[tauri::command]
pub fn append_log(app: AppHandle, keyword: String, message: String) -> Result<(), String> {
    let path = log_path(&app)?;
    ensure_parent(&path)?;
    let timestamp = Local::now().format("%Y-%m-%d %H:%M:%S").to_string();
    let line = format!("{} | {} | {}\n", timestamp, keyword, message);
    let mut file = fs::OpenOptions::new()
        .append(true)
        .create(true)
        .open(&path)
        .map_err(|e| e.to_string())?;
    file.write_all(line.as_bytes()).map_err(|e| e.to_string())
}

#[tauri::command]
pub fn get_last_push_status(app: AppHandle) -> Result<Option<String>, String> {
    let path = log_path(&app)?;
    if !path.exists() {
        return Ok(None);
    }
    let content = fs::read_to_string(&path).map_err(|e| e.to_string())?;
    let last = content
        .lines()
        .filter(|line| {
            let parts: Vec<&str> = line.splitn(3, " | ").collect();
            parts.get(1).map(|k| *k == "PUSH").unwrap_or(false)
        })
        .last()
        .map(|line| line.to_string());
    Ok(last)
}

#[tauri::command]
pub fn open_url(app: AppHandle, url: String) -> Result<(), String> {
    tauri_plugin_opener::open_url(url, None::<&str>).map_err(|e| e.to_string())
}
