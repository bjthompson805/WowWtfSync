#pragma once
#include <string>

inline constexpr const char* API_BASE = "https://wowwtfsync.com";

struct LoginResult {
    bool        success = false;
    std::string error;
    std::string accessToken;
    std::string refreshToken;
    std::string username;
    std::string email;
};

struct MeResult {
    bool        success = false;
    std::string error;
    std::string username;
    std::string email;
    std::string subStatus;        // "active", "cancelled", ""
    std::string currentPeriodEnd; // ISO date string or ""
    std::string licenseKey;       // "" if none
};

struct RefreshResult {
    bool        success = false;
    std::string error;
    std::string accessToken;
    std::string refreshToken;
};

class ApiClient {
public:
    static LoginResult   login(const std::string& email, const std::string& password);
    static MeResult      me(const std::string& accessToken);
    static RefreshResult refresh(const std::string& refreshToken);
    static bool          logout(const std::string& accessToken, const std::string& refreshToken);
    static bool          forgotPassword(const std::string& email, std::string& outMessage);
};
