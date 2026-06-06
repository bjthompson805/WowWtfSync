#define CPPHTTPLIB_OPENSSL_SUPPORT
#include "httplib.h"
#include "ApiClient.h"
#include <nlohmann/json.hpp>

using json = nlohmann::json;

// Build a client pointed at the API base URL.
static httplib::Client makeClient()
{
    httplib::Client c(API_BASE);
    c.set_connection_timeout(10);
    c.set_read_timeout(20);
    c.set_write_timeout(10);
    return c;
}

// ── login ──────────────────────────────────────────────────────────────────

LoginResult ApiClient::login(const std::string& email, const std::string& password)
{
    LoginResult r;
    try {
        auto c   = makeClient();
        auto res = c.Post("/api/auth/login",
                          json({{"email", email}, {"password", password}}).dump(),
                          "application/json");
        if (!res) { r.error = "Could not connect to server."; return r; }

        auto j = json::parse(res->body);
        if (res->status != 200) {
            r.error = j.value("error", "Login failed.");
            return r;
        }
        r.success      = true;
        r.accessToken  = j.at("accessToken").get<std::string>();
        r.refreshToken = j.at("refreshToken").get<std::string>();
        r.username     = j.at("user").at("username").get<std::string>();
        r.email        = j.at("user").at("email").get<std::string>();
    } catch (const std::exception& e) {
        r.success = false;
        r.error   = std::string("Parse error: ") + e.what();
        fprintf(stderr, "[ApiClient::login] exception: %s\n", e.what());
    } catch (...) { r.success = false; r.error = "Unexpected error during login."; }
    return r;
}

// ── me ─────────────────────────────────────────────────────────────────────

MeResult ApiClient::me(const std::string& accessToken)
{
    MeResult r;
    try {
        auto c = makeClient();
        httplib::Headers headers = {{"Authorization", "Bearer " + accessToken}};
        auto res = c.Get("/api/auth/me", headers);
        if (!res) { r.error = "Could not connect to server."; return r; }

        auto j = json::parse(res->body);
        if (res->status != 200) {
            r.error = j.value("error", "Failed to fetch account info.");
            return r;
        }
        // Helper: return string value or "" when the field is absent or null.
        auto strOrEmpty = [&](const char* key) -> std::string {
            auto it = j.find(key);
            if (it == j.end() || it->is_null()) return "";
            return it->get<std::string>();
        };

        r.success          = true;
        r.username         = strOrEmpty("username");
        r.email            = strOrEmpty("email");
        r.subStatus        = strOrEmpty("sub_status");
        r.currentPeriodEnd = strOrEmpty("current_period_end");
        r.licenseKey = strOrEmpty("license_key");
    } catch (const std::exception& e) {
        r.success = false;
        r.error   = std::string("Parse error: ") + e.what();
    } catch (...) {
        r.success = false;
        r.error   = "Unexpected error.";
    }
    return r;
}

// ── refresh ────────────────────────────────────────────────────────────────

RefreshResult ApiClient::refresh(const std::string& refreshToken)
{
    RefreshResult r;
    try {
        auto c   = makeClient();
        auto res = c.Post("/api/auth/refresh",
                          json({{"refreshToken", refreshToken}}).dump(),
                          "application/json");
        if (!res) { r.error = "Could not connect."; return r; }

        auto j = json::parse(res->body);
        if (res->status != 200) {
            r.error = j.value("error", "Token refresh failed.");
            return r;
        }
        r.success      = true;
        r.accessToken  = j.at("accessToken").get<std::string>();
        r.refreshToken = j.at("refreshToken").get<std::string>();
    } catch (...) { r.error = "Unexpected error during token refresh."; }
    return r;
}

// ── logout ─────────────────────────────────────────────────────────────────

bool ApiClient::logout(const std::string& accessToken, const std::string& refreshToken)
{
    try {
        auto c = makeClient();
        httplib::Headers headers = {{"Authorization", "Bearer " + accessToken}};
        c.Post("/api/auth/logout", headers,
               json({{"refreshToken", refreshToken}}).dump(),
               "application/json");
        return true;
    } catch (...) { return false; }
}

// ── forgotPassword ─────────────────────────────────────────────────────────

bool ApiClient::forgotPassword(const std::string& email, std::string& outMessage)
{
    try {
        auto c   = makeClient();
        auto res = c.Post("/api/auth/forgot-password",
                          json({{"email", email}}).dump(),
                          "application/json");
        if (!res) { outMessage = "Could not connect to server."; return false; }

        auto j  = json::parse(res->body);
        outMessage = j.value("message", j.value("error", "Unknown response."));
        return res->status == 200;
    } catch (...) { outMessage = "Unexpected error."; return false; }
}
