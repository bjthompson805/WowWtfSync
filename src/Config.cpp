#include "Config.h"
#include <fstream>
#include <nlohmann/json.hpp>

using json = nlohmann::json;

bool Config::load(const std::string& path, AppConfig& out)
{
    std::ifstream f(path);
    if (!f) return false;

    try {
        json j = json::parse(f);
        out.wtfPath = j.value("wtfPath", "");
        out.characters.clear();
        for (auto& c : j.value("characters", json::array())) {
            CharacterEntry e;
            e.characterName = c.value("characterName", "");
            e.realmName     = c.value("realmName",     "");
            e.account       = c.value("account",       "");
            e.faction       = c.value("faction",       "Alliance");
            e.enabled       = c.value("enabled",       true);
            out.characters.push_back(std::move(e));
        }
    } catch (...) { return false; }
    return true;
}

bool Config::save(const std::string& path, const AppConfig& cfg)
{
    json j;
    j["wtfPath"] = cfg.wtfPath;
    j["characters"] = json::array();
    for (auto& c : cfg.characters) {
        j["characters"].push_back({
            {"characterName", c.characterName},
            {"realmName",     c.realmName},
            {"account",       c.account},
            {"faction",       c.faction},
            {"enabled",       c.enabled}
        });
    }
    std::ofstream f(path);
    if (!f) return false;
    f << j.dump(4);
    return true;
}
