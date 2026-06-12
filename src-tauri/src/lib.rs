mod auth;
mod commands;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::scan_wtf_folder,
            commands::get_config,
            commands::save_config,
            commands::get_config_path,
            commands::run_push,
            commands::append_log,
            commands::get_last_push_status,
            auth::auth_login,
            auth::auth_refresh,
            auth::auth_get_me,
            auth::auth_get_session,
            auth::auth_logout,
            commands::open_url,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
