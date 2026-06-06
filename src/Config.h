#pragma once
#include <string>
#include <vector>

struct CharacterEntry {
    std::string characterName;
    std::string realmName;
    std::string account;
    std::string faction;   // "Alliance" or "Horde"
    bool        enabled = true;

    bool operator==(const CharacterEntry& o) const {
        return characterName == o.characterName
            && realmName     == o.realmName
            && account       == o.account
            && faction       == o.faction
            && enabled       == o.enabled;
    }
};

struct AppConfig {
    std::string                 wtfPath;
    std::vector<CharacterEntry> characters;

    bool operator==(const AppConfig& o) const {
        return wtfPath == o.wtfPath && characters == o.characters;
    }
};

class Config {
public:
    static bool load(const std::string& path, AppConfig& out);
    static bool save(const std::string& path, const AppConfig& cfg);
};
