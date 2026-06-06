#pragma once
#include <atomic>
#include <future>
#include <string>
#include "ApiClient.h"

enum class AuthState {
    Checking,        // restoring saved session — show spinner
    LoggedOut,       // no valid session — show login screen
    NoSubscription,  // logged in but subscription is not active
    LoggedIn         // fully authenticated and subscribed
};

// Everything the UI needs to know about the current user.
struct UserInfo {
    std::string username;
    std::string email;
    std::string subStatus;
    std::string currentPeriodEnd;
    std::string licenseKey;
    std::string accessToken;
    std::string refreshToken;
};

class AuthSession {
public:
    AuthSession() = default;
    explicit AuthSession(const std::string& sessionPath);

    // Two-phase init: call after the session path is known, before tryRestore().
    void init(const std::string& sessionPath) { m_sessionPath = sessionPath; }
    ~AuthSession();

    AuthState       state()     const { return m_state; }
    const UserInfo& userInfo()  const { return m_user; }
    bool            isBusy()    const { return m_busy.load(); }
    const std::string& error()  const { return m_error; }

    // ── Startup ────────────────────────────────────────────────────────────
    // Try to resume a previously saved session.  Call once at startup.
    void tryRestore();

    // ── Login screen ───────────────────────────────────────────────────────
    void attemptLogin(const std::string& email, const std::string& password);
    void requestForgotPassword(const std::string& email);
    const std::string& forgotPasswordMessage() const { return m_forgotMsg; }

    // ── Account dialog ─────────────────────────────────────────────────────
    // Reload full account info from /api/auth/me (for the account dialog).
    void refreshMe();

    // ── Sign out ───────────────────────────────────────────────────────────
    void signOut();

private:
    std::string m_sessionPath;
    AuthState   m_state = AuthState::Checking;
    UserInfo    m_user;
    std::string m_error;
    std::string m_forgotMsg;

    std::atomic<bool> m_busy{false};
    std::future<void> m_future;

    void saveSession();
    void clearSession();

    // After a successful login/refresh: set tokens, call /me, set state.
    void finishAuth(const std::string& accessToken, const std::string& refreshToken,
                    const std::string& username, const std::string& email);
};
