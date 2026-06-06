#pragma once
#include <string>
#include <cstdio>

#ifndef _WIN32
#  include <sys/wait.h>
#endif

struct ProcessResult {
    std::string output;   // combined stdout + stderr
    int         exitCode = -1;
};

// Run cmd in a shell, capture combined stdout+stderr, return exit code.
// cmd should already have "2>&1" appended if stderr capture is desired.
inline ProcessResult runProcess(const std::string& cmd)
{
    ProcessResult r;
#ifdef _WIN32
    FILE* pipe = _popen(cmd.c_str(), "r");
#else
    FILE* pipe = popen(cmd.c_str(), "r");
#endif
    if (!pipe) return r;

    char buf[256];
    while (fgets(buf, sizeof(buf), pipe))
        r.output += buf;

#ifdef _WIN32
    r.exitCode = _pclose(pipe);
#else
    int status = pclose(pipe);
    r.exitCode = WIFEXITED(status) ? WEXITSTATUS(status) : -1;
#endif
    return r;
}

// Try common lua interpreter names; return the first one found in PATH, or "".
inline std::string findLua()
{
    for (const char* cand : {"lua5.4", "lua54", "lua5.3", "lua"}) {
#ifdef _WIN32
        std::string check = std::string("where ") + cand + " >nul 2>&1";
#else
        std::string check = std::string("which ") + cand + " >/dev/null 2>&1";
#endif
        if (std::system(check.c_str()) == 0)
            return cand;
    }
    return {};
}
