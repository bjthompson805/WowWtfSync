use serde::{Deserialize, Serialize};
use std::fs;
use std::path::PathBuf;
use tauri::AppHandle;
use tauri::Manager;

const API_BASE: &str = "https://wowwtfsync.com/api";
const SESSION_FILE: &str = "session.json";

// ─── Types ───────────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct SessionUser {
    pub id: u64,
    pub username: String,
    pub email: String,
}

#[derive(Debug, Serialize, Deserialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct Session {
    pub access_token: String,
    pub refresh_token: String,
    pub user: SessionUser,
}

/// The full account info returned by GET /api/auth/me
#[derive(Debug, Serialize, Deserialize, Clone)]
#[serde(rename_all = "snake_case")]
pub struct MeInfo {
    pub id: u64,
    pub username: String,
    pub email: String,
    pub sub_status: Option<String>,
    pub current_period_end: Option<String>,
    pub cancel_at_period_end: Option<i64>,
    pub license_key: Option<String>,
}

#[derive(Debug, Serialize, Deserialize)]
struct LoginRequest {
    email: String,
    password: String,
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
struct LoginResponse {
    access_token: String,
    refresh_token: String,
    user: SessionUser,
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
struct RefreshRequest {
    refresh_token: String,
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
struct RefreshResponse {
    access_token: String,
    refresh_token: String,
}

#[derive(Debug, Serialize, Deserialize)]
struct ApiError {
    error: String,
}

// ─── Path helpers ─────────────────────────────────────────────────────────────

fn session_path(app: &AppHandle) -> Result<PathBuf, String> {
    app.path()
        .app_data_dir()
        .map(|d| d.join(SESSION_FILE))
        .map_err(|e| e.to_string())
}

// ─── Session persistence ──────────────────────────────────────────────────────

pub fn load_session(app: &AppHandle) -> Option<Session> {
    let path = session_path(app).ok()?;
    let content = fs::read_to_string(path).ok()?;
    serde_json::from_str(&content).ok()
}

fn save_session(app: &AppHandle, session: &Session) -> Result<(), String> {
    let path = session_path(app)?;
    if let Some(parent) = path.parent() {
        fs::create_dir_all(parent).map_err(|e| e.to_string())?;
    }
    let content = serde_json::to_string(session).map_err(|e| e.to_string())?;
    fs::write(path, content).map_err(|e| e.to_string())
}

fn delete_session(app: &AppHandle) {
    if let Ok(path) = session_path(app) {
        let _ = fs::remove_file(path);
    }
}

// ─── HTTP helpers ─────────────────────────────────────────────────────────────

fn parse_api_error(body: &str, status: u16) -> String {
    if let Ok(e) = serde_json::from_str::<ApiError>(body) {
        e.error
    } else {
        format!("Server returned status {}", status)
    }
}

// ─── Commands ────────────────────────────────────────────────────────────────

/// Attempt login. Returns the session (with active subscription required).
#[tauri::command]
pub fn auth_login(app: AppHandle, email: String, password: String) -> Result<Session, String> {
    let body = serde_json::to_string(&LoginRequest { email, password })
        .map_err(|e| e.to_string())?;

    let response = ureq::post(&format!("{}/auth/login", API_BASE))
        .set("Content-Type", "application/json")
        .send_string(&body);

    match response {
        Ok(resp) => {
            let text = resp.into_string().map_err(|e| e.to_string())?;
            let login: LoginResponse =
                serde_json::from_str(&text).map_err(|e| e.to_string())?;

            // Check subscription before accepting login
            let me = fetch_me(&login.access_token)?;
            let sub_status = me.sub_status.as_deref().unwrap_or("inactive");
            if sub_status != "active" {
                return Err(
                    "Your account does not have an active subscription. \
                     Please subscribe at wowwtfsync.com to use this app."
                        .to_string(),
                );
            }

            let session = Session {
                access_token: login.access_token,
                refresh_token: login.refresh_token,
                user: login.user,
            };
            save_session(&app, &session)?;
            Ok(session)
        }
        Err(ureq::Error::Status(status, resp)) => {
            let text = resp.into_string().unwrap_or_default();
            Err(parse_api_error(&text, status))
        }
        Err(e) => Err(format!("Network error: {}", e)),
    }
}

/// Try to refresh using a stored refresh token. Returns the updated session.
#[tauri::command]
pub fn auth_refresh(app: AppHandle) -> Result<Session, String> {
    let mut session = load_session(&app).ok_or("Not logged in")?;

    let body = serde_json::to_string(&RefreshRequest {
        refresh_token: session.refresh_token.clone(),
    })
    .map_err(|e| e.to_string())?;

    let response = ureq::post(&format!("{}/auth/refresh", API_BASE))
        .set("Content-Type", "application/json")
        .send_string(&body);

    match response {
        Ok(resp) => {
            let text = resp.into_string().map_err(|e| e.to_string())?;
            let refreshed: RefreshResponse =
                serde_json::from_str(&text).map_err(|e| e.to_string())?;
            session.access_token = refreshed.access_token;
            session.refresh_token = refreshed.refresh_token;
            save_session(&app, &session)?;
            Ok(session)
        }
        Err(ureq::Error::Status(status, resp)) => {
            let text = resp.into_string().unwrap_or_default();
            Err(parse_api_error(&text, status))
        }
        Err(e) => Err(format!("Network error: {}", e)),
    }
}

/// Returns full account/subscription info for the currently logged-in user.
#[tauri::command]
pub fn auth_get_me(app: AppHandle) -> Result<MeInfo, String> {
    // Try with stored token; if 401, attempt a refresh first.
    let session = load_session(&app).ok_or("Not logged in")?;
    match fetch_me(&session.access_token) {
        Ok(me) => Ok(me),
        Err(_) => {
            // Try refresh
            let refreshed = auth_refresh(app)?;
            fetch_me(&refreshed.access_token)
        }
    }
}

/// Returns the stored session if one exists (used on app startup).
#[tauri::command]
pub fn auth_get_session(app: AppHandle) -> Option<Session> {
    load_session(&app)
}

/// Logs out: calls the server logout endpoint and removes the local session.
#[tauri::command]
pub fn auth_logout(app: AppHandle) -> Result<(), String> {
    if let Some(session) = load_session(&app) {
        let body = serde_json::json!({ "refreshToken": session.refresh_token }).to_string();
        // Best-effort — don't fail the logout if the network call fails.
        let _ = ureq::post(&format!("{}/auth/logout", API_BASE))
            .set("Content-Type", "application/json")
            .set("Authorization", &format!("Bearer {}", session.access_token))
            .send_string(&body);
    }
    delete_session(&app);
    Ok(())
}

// ─── Internal helpers ─────────────────────────────────────────────────────────

pub fn fetch_me(access_token: &str) -> Result<MeInfo, String> {
    let response = ureq::get(&format!("{}/auth/me", API_BASE))
        .set("Authorization", &format!("Bearer {}", access_token))
        .call();

    match response {
        Ok(resp) => {
            let text = resp.into_string().map_err(|e| e.to_string())?;
            serde_json::from_str(&text).map_err(|e| e.to_string())
        }
        Err(ureq::Error::Status(status, resp)) => {
            let text = resp.into_string().unwrap_or_default();
            Err(parse_api_error(&text, status))
        }
        Err(e) => Err(format!("Network error: {}", e)),
    }
}
