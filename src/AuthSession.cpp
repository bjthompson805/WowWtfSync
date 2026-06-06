#include "AuthSession.h"
#include <fstream>
#include <thread>
#include <nlohmann/json.hpp>

using json = nlohmann::json;

AuthSession::AuthSession(const std::string& sessionPath)
    : m_sessionPath(sessionPath)
{}

AuthSession::~AuthSession()
{
    // Join background thread before destruction.
    if (m_future.valid()) m_future.get();
}

// ── Session persistence ───────────────────────────────────────────────────────

void AuthSession::saveSession()
{
    json j = {
        {"refreshToken", m_user.refreshToken},
        {"username",     m_user.username},
        {"email",        m_user.email}
    };
    std::ofstream f(m_sessionPath);
    if (f) f << j.dump(2);
}

void AuthSession::clearSession()
{
    std::remove(m_sessionPath.c_str());
    m_user = {};
}

// ── Startup restore ───────────────────────────────────────────────────────────

void AuthSession::tryRestore()
{
    // Try to load a saved refresh token from disk.
    std::string savedRefresh, savedUsername, savedEmail;
    {
        std::ifstream f(m_sessionPath);
        if (f) {
            try {
                auto j      = json::parse(f);
                savedRefresh  = j.value("refreshToken", "");
                savedUsername = j.value("username",     "");
                savedEmail    = j.value("email",        "");
            } catch (...) {}
        }
    }

    if (savedRefresh.empty()) {
        m_state = AuthState::LoggedOut;
        return;
    }

    // Restore pre-loaded display values immediately so the menu bar can show
    // them without waiting for the network round-trip.
    m_user.username     = savedUsername;
    m_user.email        = savedEmail;
    m_user.refreshToken = savedRefresh;

    m_busy = true;
    m_future = std::async(std::launch::async, [this, savedRefresh]() {
        // 1. Refresh the access token.
        RefreshResult rr = ApiClient::refresh(savedRefresh);
        if (!rr.success) {
            clearSession();
            m_error = rr.error;
            m_state = AuthState::LoggedOut;
            m_busy  = false;
            return;
        }
        // 2. Fetch account/subscription info.
        MeResult mr = ApiClient::me(rr.accessToken);
        if (!mr.success) {
            clearSession();
            m_error = mr.error;
            m_state = AuthState::LoggedOut;
            m_busy  = false;
            return;
        }

        m_user.accessToken     = rr.accessToken;
        m_user.refreshToken    = rr.refreshToken;
        m_user.username        = mr.username;
        m_user.email           = mr.email;
        m_user.subStatus       = mr.subStatus;
        m_user.currentPeriodEnd = mr.currentPeriodEnd;
        m_user.licenseKey      = mr.licenseKey;

        saveSession();
        m_state = (mr.subStatus == "active") ? AuthState::LoggedIn
                                              : AuthState::NoSubscription;
        m_busy = false;
    });
}

// ── Login ─────────────────────────────────────────────────────────────────────

void AuthSession::attemptLogin(const std::string& email, const std::string& password)
{
    if (m_busy) return;
    m_error.clear();
    m_busy = true;

    m_future = std::async(std::launch::async, [this, email, password]() {
        LoginResult lr = ApiClient::login(email, password);
        if (!lr.success) {
            m_error = lr.error;
            m_busy  = false;
            return;
        }

        MeResult mr = ApiClient::me(lr.accessToken);
        if (!mr.success) {
            m_error = mr.error;
            m_busy  = false;
            return;
        }

        m_user.accessToken      = lr.accessToken;
        m_user.refreshToken     = lr.refreshToken;
        m_user.username         = mr.username;
        m_user.email            = mr.email;
        m_user.subStatus        = mr.subStatus;
        m_user.currentPeriodEnd = mr.currentPeriodEnd;
        m_user.licenseKey       = mr.licenseKey;

        saveSession();
        m_state = (mr.subStatus == "active") ? AuthState::LoggedIn
                                              : AuthState::NoSubscription;
        m_busy = false;
    });
}

// ── Forgot password ───────────────────────────────────────────────────────────

void AuthSession::requestForgotPassword(const std::string& email)
{
    if (m_busy) return;
    m_forgotMsg.clear();
    m_busy = true;

    m_future = std::async(std::launch::async, [this, email]() {
        std::string msg;
        ApiClient::forgotPassword(email, msg);
        m_forgotMsg = msg;
        m_busy      = false;
    });
}

// ── refreshMe ─────────────────────────────────────────────────────────────────

void AuthSession::refreshMe()
{
    if (m_busy || m_user.accessToken.empty()) return;
    m_busy = true;

    m_future = std::async(std::launch::async, [this]() {
        MeResult mr = ApiClient::me(m_user.accessToken);
        if (mr.success) {
            m_user.subStatus        = mr.subStatus;
            m_user.currentPeriodEnd = mr.currentPeriodEnd;
            m_user.licenseKey       = mr.licenseKey;
        }
        m_busy = false;
    });
}

// ── Sign out ──────────────────────────────────────────────────────────────────

void AuthSession::signOut()
{
    // Fire-and-forget logout call; don't block the UI.
    std::string at = m_user.accessToken;
    std::string rt = m_user.refreshToken;
    std::thread([at, rt]() {
        ApiClient::logout(at, rt);
    }).detach();

    clearSession();
    m_error.clear();
    m_state = AuthState::LoggedOut;
}
