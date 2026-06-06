#pragma once
#include <string>

// Keywords embedded in log entries so they can be searched later.
namespace LogKeyword {
    inline constexpr const char* PUSH    = "[PUSH]";
    inline constexpr const char* ADD     = "[ADD]";
    inline constexpr const char* REMOVE  = "[REMOVE]";
    inline constexpr const char* CONFIG  = "[CONFIG]";
}

class Logger {
public:
    // Call once at startup with the full path to log.txt.
    static void init(const std::string& path);

    // Append a timestamped line.  keyword should be one of LogKeyword::*.
    static void log(const std::string& keyword, const std::string& message);

    // Return the most recent line that contains the given keyword, or "".
    static std::string lastLineWith(const std::string& keyword);

private:
    static std::string s_path;
};
