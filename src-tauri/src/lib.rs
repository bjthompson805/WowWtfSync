mod commands;

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_dialog::init())
        .invoke_handler(tauri::generate_handler![
            commands::scan_wtf_folder,
            commands::get_config,
            commands::save_config,
            commands::get_config_path,
            commands::run_push,
            commands::append_log,
            commands::get_last_push_status,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
