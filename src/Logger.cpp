#include "Logger.h"
#include <ctime>
#include <fstream>
#include <sstream>

std::string Logger::s_path;

void Logger::init(const std::string& path) { s_path = path; }

void Logger::log(const std::string& keyword, const std::string& message)
{
    if (s_path.empty()) return;
    std::ofstream f(s_path, std::ios::app);
    if (!f) return;

    std::time_t t = std::time(nullptr);
    char ts[32];
    std::strftime(ts, sizeof(ts), "%Y-%m-%d %H:%M:%S", std::localtime(&t));
    f << ts << " " << keyword << " " << message << "\n";
}

std::string Logger::lastLineWith(const std::string& keyword)
{
    if (s_path.empty()) return {};
    std::ifstream f(s_path);
    if (!f) return {};

    std::string last, line;
    while (std::getline(f, line))
        if (line.find(keyword) != std::string::npos)
            last = line;
    return last;
}
