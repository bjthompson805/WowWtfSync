#include "PushDialog.h"
#include "Logger.h"
#include "imgui.h"
#include <sstream>

void PushDialog::show(int addonIndex,
                      const std::string& charArg,
                      const std::string& charLabel,
                      const std::string& lua,
                      const std::string& pushLua,
                      const std::string& cfgPath)
{
    if (addonIndex < 0) {
        addonName = "All Addons";
        addonKey  = "all";
    } else {
        addonName = ADDON_NAMES[addonIndex];
        addonKey  = ADDON_KEYS[addonIndex];
    }
    characterArg   = charArg;
    characterLabel = charLabel;
    luaInterpreter = lua;
    pushLuaPath    = pushLua;
    configPath     = cfgPath;
    state          = PushState::Idle;
    result.clear();
    // Use the human-readable title as the popup ID so ImGui shows it in the
    // title bar.  The ##tag suffix keeps the ImGui ID unique.
    popupId = "Push " + addonName + " - " + characterLabel + "##PushDlg";
    open = true;
}

// ─── Description text ────────────────────────────────────────────────────────

std::string PushDialog::addonDescription(const std::string& name,
                                          const std::string& charLabel,
                                          const std::string& charArg)
{
    bool allChars = (charLabel == "All Characters");
    if (name == "All Addons")
        return allChars
            ? "Push all supported addon data (Altoholic, Auctionator, Bagnon, NovaWorldBuffs, TitanGold) "
              "from all accounts."
            : "Push all supported addon data (Altoholic, Auctionator, Bagnon, NovaWorldBuffs, TitanGold) "
              "for " + charArg + " to all other accounts.";
    if (name == "Altoholic")
        return allChars
            ? "Push Altoholic data from all accounts, combining the data."
            : "Push Altoholic data for " + charArg + " by combining the data to/from all accounts.";
    if (name == "Auctionator")
        return "This will copy any scan days for an item that haven't been seen by the "
               "destination account so that missing days will be merged in. "
               "This process may take a minute.";
    if (name == "Bagnon")
        return "This will allow you to view characters' bags/banks that are on other accounts.";
    if (name == "NovaWorldBuffs")
        return "This will allow you to view characters' world buffs and raid lockouts "
               "that are on other accounts.";
    if (name == "TitanGold")
        return "This will allow you to view characters' gold that are on other accounts.";
    return {};
}

// ─── Launch push in background thread ────────────────────────────────────────

void PushDialog::launchPush()
{
    state = PushState::Running;

#ifdef _WIN32
    auto q = [](const std::string& s){ return "\"" + s + "\""; };
#else
    auto q = [](const std::string& s){
        std::string out;
        for (char c : s) { if (c == '\'') out += "'\\''"; else out += c; }
        return "'" + out + "'";
    };
#endif

    std::string cmd = luaInterpreter
        + " " + q(pushLuaPath)
        + " " + addonKey
        + " " + q(configPath)
        + " " + q(characterArg)
        + " 2>&1";

    future = std::async(std::launch::async, [cmd]() { return runProcess(cmd); });
}

// ─── Render ──────────────────────────────────────────────────────────────────

bool PushDialog::render()
{
    if (!open) return false;

    ImGui::OpenPopup(popupId.c_str());

    ImVec2 center = ImGui::GetMainViewport()->GetCenter();
    ImGui::SetNextWindowPos(center, ImGuiCond_Always, ImVec2(0.5f, 0.5f));
    ImGui::SetNextWindowSize(ImVec2(540, 0), ImGuiCond_Always);

    bool dismissed = false;

    // Passing &open gives ImGui the X button in the title bar.
    // When the user clicks X, ImGui sets open = false and closes the popup.
    if (ImGui::BeginPopupModal(popupId.c_str(), &open,
            ImGuiWindowFlags_AlwaysAutoResize))
    {
        // ── Poll background thread ────────────────────────────────────────
        if (state == PushState::Running &&
            future.valid() &&
            future.wait_for(std::chrono::seconds(0)) == std::future_status::ready)
        {
            ProcessResult pr = future.get();
            result = pr.output;
            state  = (pr.exitCode == 0) ? PushState::Done : PushState::Error;
            if (state == PushState::Done) {
                Logger::log(LogKeyword::PUSH,
                    addonName + " | " + characterLabel + " | success");
            }
        }

        // ── Body (idle / running / done / error) ─────────────────────────
        if (state == PushState::Idle || state == PushState::Running) {
            bool allChars = (characterLabel == "All Characters");
            if (addonName != "Altoholic" && addonName != "All Addons") {
                std::string sentence1 = "Push " + addonName + " data from "
                    + (allChars ? "each character" : characterArg)
                    + " to all accounts that are not the same as the character's account.";
                ImGui::TextWrapped("%s", sentence1.c_str());
                ImGui::Spacing();
            }
            ImGui::TextWrapped("%s", addonDescription(addonName, characterLabel, characterArg).c_str());
            ImGui::Spacing();
            ImGui::TextWrapped("Make sure to log out of all characters that are being "
                               "pushed to, otherwise the data will be overwritten when "
                               "logout occurs.");
            if (addonName == "Altoholic") {
                ImGui::Spacing();
                ImGui::TextWrapped("*Aggregation of mail sent to your own character "
                                   "from another account is also supported.");
            }
            ImGui::Spacing();
            ImGui::Separator();
            ImGui::Spacing();

            if (state == PushState::Running) {
                ImGui::TextUnformatted("Pushing, please wait...");
                static const char* frames[] = {"|", "/", "-", "\\"};
                ImGui::SameLine();
                ImGui::TextUnformatted(frames[(int)(ImGui::GetTime() * 8) % 4]);
            } else {
                float btnW = 120.f;
                ImGui::SetCursorPosX(
                    ImGui::GetCursorPosX()
                    + ImGui::GetContentRegionAvail().x - btnW);
                if (ImGui::Button("Push", ImVec2(btnW, 0)))
                    launchPush();
            }
        }
        else if (state == PushState::Done) {
            ImGui::TextWrapped("%s", result.c_str());
            ImGui::Spacing();
            if (ImGui::Button("OK", ImVec2(120, 0)))
                dismissed = true;
        }
        else { // Error
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1, 0.3f, 0.3f, 1));
            ImGui::TextWrapped("Error:\n%s", result.c_str());
            ImGui::PopStyleColor();
            ImGui::Spacing();
            if (ImGui::Button("OK", ImVec2(120, 0)))
                dismissed = true;
        }

        ImGui::EndPopup();
    }

    // open may have been set to false by ImGui's X button
    if (!open || dismissed) {
        open  = false;
        state = PushState::Idle;
        return true;
    }
    return false;
}
