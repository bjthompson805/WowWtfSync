#pragma once
#include <cstdlib>
#include <string>

#ifdef _WIN32
#  ifndef WIN32_LEAN_AND_MEAN
#    define WIN32_LEAN_AND_MEAN
#  endif
#  include <windows.h>
#  include <shellapi.h>
#endif

// Web URLs
inline constexpr const char* URL_REGISTER  = "https://wowwtfsync.com/register.html";
inline constexpr const char* URL_FORGOT_PW = "https://wowwtfsync.com/forgot-password.html";
inline constexpr const char* URL_SUBSCRIBE = "https://wowwtfsync.com/subscribe.html";
inline constexpr const char* URL_DASHBOARD = "https://wowwtfsync.com/dashboard.html";

// Open a URL in the system's default browser, cross-platform.
inline void openUrl(const char* url)
{
#if defined(_WIN32)
    ShellExecuteA(nullptr, "open", url, nullptr, nullptr, SW_SHOW);
#elif defined(__APPLE__)
    std::string cmd = std::string("open '") + url + "' &";
    std::system(cmd.c_str());
#else
    std::string cmd = std::string("xdg-open '") + url + "' &";
    std::system(cmd.c_str());
#endif
}
