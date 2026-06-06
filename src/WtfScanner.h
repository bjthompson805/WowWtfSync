#pragma once
#include <filesystem>
#include <map>
#include <regex>
#include <string>
#include <vector>

namespace fs = std::filesystem;

// Returns map of accountName → list of "CharacterName-RealmName" strings.
// Scans wtfPath/Account/ for dirs matching \d{8}#\d+, then each realm subdir
// (excluding SavedVariables) for character subdirs.
inline std::map<std::string, std::vector<std::string>>
scanWtf(const std::string& wtfPath)
{
    std::map<std::string, std::vector<std::string>> result;
    if (wtfPath.empty()) return result;

    fs::path accountDir = fs::path(wtfPath) / "Account";
    if (!fs::is_directory(accountDir)) return result;

    std::regex accountRe(R"(\d{8}#\d+)");

    for (auto& accountEntry : fs::directory_iterator(accountDir)) {
        if (!accountEntry.is_directory()) continue;
        std::string accountName = accountEntry.path().filename().string();
        if (!std::regex_match(accountName, accountRe)) continue;

        std::vector<std::string> chars;
        for (auto& realmEntry : fs::directory_iterator(accountEntry.path())) {
            if (!realmEntry.is_directory()) continue;
            std::string realmName = realmEntry.path().filename().string();
            if (realmName == "SavedVariables") continue;

            for (auto& charEntry : fs::directory_iterator(realmEntry.path())) {
                if (!charEntry.is_directory()) continue;
                std::string charName = charEntry.path().filename().string();
                chars.push_back(charName + "-" + realmName);
            }
        }
        std::sort(chars.begin(), chars.end());
        result[accountName] = std::move(chars);
    }
    return result;
}
