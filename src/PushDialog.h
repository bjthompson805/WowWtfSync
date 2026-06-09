#pragma once
#include <future>
#include <string>
#include "Process.h"

// Addon display names in canonical order.
inline constexpr const char* ADDON_NAMES[] = {
    "Altoholic", "Auctionator", "Bagnon", "NovaWorldBuffs", "TitanGold"
};
inline constexpr int ADDON_COUNT = 5;

// Lowercase keys used by push.lua.
inline constexpr const char* ADDON_KEYS[] = {
    "altoholic", "auctionator", "bagnon", "novaworldbuffs", "titangold"
};

enum class PushState { Idle, Running, Done, Error };

struct PushDialog {
    bool        open       = false;
    std::string addonName;          // display name, e.g. "Bagnon"
    std::string addonKey;           // push.lua key, e.g. "bagnon"
    std::string characterArg;       // "all" or "CharName-Realm-Account"
    std::string characterLabel;     // display label for the header
    std::string luaInterpreter;
    std::string pushLuaPath;
    std::string configPath;

    PushState   state     = PushState::Idle;
    float       dpiScale  = 1.0f;
    std::string result;             // success or error message from push.lua
    std::string popupId;            // ImGui popup ID (title + ##tag)
    std::future<ProcessResult> future;

    // Open the dialog for a push operation.
    // characterArg = "all"  OR  "CharName-Realm-Account"
    // characterLabel = "All Characters"  OR  "CharName-Realm"
    void show(int addonIndex,
              const std::string& charArg,
              const std::string& charLabel,
              const std::string& lua,
              const std::string& pushLua,
              const std::string& cfgPath);

    // Call every frame.  Returns true when the dialog is finished
    // and should be dismissed (user clicked OK after result, or closed).
    bool render();

private:
    void launchPush();
    void renderBody();
    static std::string addonDescription(const std::string& name,
                                        const std::string& charLabel,
                                        const std::string& charArg);
};
