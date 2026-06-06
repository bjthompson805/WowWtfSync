#include "App.h"
#include "Config.h"
#include "Logger.h"
#include "WtfScanner.h"
#include "imgui.h"
#include <SDL3/SDL.h>   // SDL_GetBasePath
#include <algorithm>
#include <filesystem>
#include <sstream>

namespace fs = std::filesystem;

// ─── Constructor ──────────────────────────────────────────────────────────────
App::App(SDL_Window* /*window*/, SDL_Renderer* /*renderer*/)
{
    // Resolve paths relative to the executable directory.
    const char* base = SDL_GetBasePath();
    std::string baseDir = base ? base : "./";

    m_configPath  = baseDir + "config.json";
    m_pushLuaPath = baseDir + "push.lua";
    m_logPath     = baseDir + "log.txt";

    Logger::init(m_logPath);
    m_luaInterp = findLua();

    // Load config; silently ignore missing file on first run.
    Config::load(m_configPath, m_cfg);
    m_savedCfg = m_cfg;   // baseline for dirty tracking
    std::snprintf(m_wtfBuf, sizeof(m_wtfBuf), "%s", m_cfg.wtfPath.c_str());

    if (!m_cfg.wtfPath.empty())
        m_scanned = scanWtf(m_cfg.wtfPath);
    for (auto& [acct, _] : m_scanned)
        m_accountList.push_back(acct);
    std::sort(m_accountList.begin(), m_accountList.end());

    refreshStatus();
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

void App::refreshStatus()
{
    m_statusText = Logger::lastLineWith(LogKeyword::PUSH);
    if (m_statusText.empty())
        m_statusText = "No pushes recorded yet.";
}

void App::applyWtfPath()
{
    m_cfg.wtfPath = m_wtfBuf;
    m_scanned     = scanWtf(m_cfg.wtfPath);
    m_accountList.clear();
    for (auto& [acct, _] : m_scanned)
        m_accountList.push_back(acct);
    std::sort(m_accountList.begin(), m_accountList.end());
    m_selAccount = -1;
    m_selChar    = -1;
    checkDirty();
    Logger::log(LogKeyword::CONFIG, "WTF path set to: " + m_cfg.wtfPath);
}

void App::requestQuit()
{
    if (m_dirty)
        m_confirmUnsavedExit = true;
    else
        m_quit = true;
}

void App::saveConfig()
{
    Config::save(m_configPath, m_cfg);
    m_savedCfg = m_cfg;   // new baseline
    m_dirty    = false;
    Logger::log(LogKeyword::CONFIG, "Configuration saved.");
}

std::vector<std::string> App::tableAccounts() const
{
    std::vector<std::string> accts;
    for (auto& c : m_cfg.characters) {
        if (std::find(accts.begin(), accts.end(), c.account) == accts.end())
            accts.push_back(c.account);
    }
    std::sort(accts.begin(), accts.end());
    return accts;
}

void App::addCharacter()
{
    if (m_selAccount < 0 || m_selAccount >= (int)m_accountList.size()) return;
    const std::string& acct = m_accountList[m_selAccount];
    auto& chars = m_scanned[acct];
    if (m_selChar < 0 || m_selChar >= (int)chars.size()) return;

    // Parse "CharName-Realm" (split on last '-')
    const std::string& cr = chars[m_selChar];
    auto dash = cr.rfind('-');
    std::string charName = (dash != std::string::npos) ? cr.substr(0, dash) : cr;
    std::string realm    = (dash != std::string::npos) ? cr.substr(dash + 1)  : "";

    // Avoid duplicates
    for (auto& e : m_cfg.characters)
        if (e.characterName == charName && e.realmName == realm && e.account == acct) {
            m_notifyDuplicate = true;
            return;
        }

    CharacterEntry e;
    e.characterName = charName;
    e.realmName     = realm;
    e.account       = acct;
    e.faction       = "Alliance";
    e.enabled       = true;
    m_cfg.characters.push_back(std::move(e));
    checkDirty();
    m_selChar = -1;
    Logger::log(LogKeyword::ADD, charName + "-" + realm + " (" + acct + ")");
}

void App::openPushAll(int addonIdx)
{
    m_pushDialog.show(addonIdx, "all", "All Characters",
                      m_luaInterp, m_pushLuaPath, m_configPath);
}

void App::openPushRow(int row, int addonIdx)
{
    auto& c = m_cfg.characters[row];
    std::string arg   = c.characterName + "-" + c.realmName + "-" + c.account;
    std::string label = c.characterName + "-" + c.realmName;
    m_pushDialog.show(addonIdx, arg, label,
                      m_luaInterp, m_pushLuaPath, m_configPath);
}

// ─── Top-level render ─────────────────────────────────────────────────────────

void App::render()
{
    // Full-screen dockspace window.
    ImGuiViewport* vp = ImGui::GetMainViewport();
    ImGui::SetNextWindowPos(vp->Pos);
    ImGui::SetNextWindowSize(vp->Size);
    ImGui::SetNextWindowBgAlpha(1.f);
    ImGui::Begin("##main", nullptr,
        ImGuiWindowFlags_NoDecoration |
        ImGuiWindowFlags_NoMove       |
        ImGuiWindowFlags_NoSavedSettings);

    // Compute proportional heights for the table (75%) and scan panels (25%).
    // Fixed overhead: wtfBar + pushAllBar + saveRow + addBtnRow + statusBar
    //                 + 4 spacings between sections + a little padding.
    {
        float fh      = ImGui::GetFrameHeightWithSpacing();
        float sp      = ImGui::GetStyle().ItemSpacing.y;
        float fixedH  = fh * 5.f + sp * 4.f + 8.f;
        float contentH = ImGui::GetWindowHeight() - fixedH;
        if (contentH < 80.f) contentH = 80.f;
        float tableH  = contentH * 0.75f;
        float panelH  = contentH * 0.25f;

        renderWtfBar();
        ImGui::Spacing();
        renderPushAllBar();
        ImGui::Spacing();
        renderCharacterTable(tableH);
        ImGui::Spacing();
        renderScanPanels(panelH);
    }

    // Status bar pinned to bottom of the main window.
    ImGui::SetCursorPosY(
        ImGui::GetWindowHeight() - ImGui::GetFrameHeightWithSpacing() - 4.f);
    renderStatusBar();

    ImGui::End();

    renderModals();
    m_pushDialog.render();

    if (m_folderPicker.render()) {
        std::snprintf(m_wtfBuf, sizeof(m_wtfBuf),
                      "%s", m_folderPicker.selected.c_str());
    }
}

// ─── WTF path bar ─────────────────────────────────────────────────────────────

void App::renderWtfBar()
{
    ImGui::TextUnformatted("WTF Folder:");
    ImGui::SameLine();

    float availW = ImGui::GetContentRegionAvail().x;
    float btnW   = 80.f;
    float spacing = ImGui::GetStyle().ItemSpacing.x;
    float inputW  = availW - btnW * 2 - spacing * 2;

    ImGui::SetNextItemWidth(inputW);
    ImGui::InputText("##wtf", m_wtfBuf, sizeof(m_wtfBuf));

    ImGui::SameLine();
    if (ImGui::Button("Browse", ImVec2(btnW, 0)))
        m_folderPicker.show(m_wtfBuf);

    ImGui::SameLine();
    if (ImGui::Button("Apply", ImVec2(btnW, 0)))
        applyWtfPath();
}

// ─── Push All row ─────────────────────────────────────────────────────────────

void App::renderPushAllBar()
{
    ImGui::TextUnformatted("Push All:");
    ImGui::SameLine();
    bool noChars = m_cfg.characters.empty();
    if (noChars) ImGui::BeginDisabled();
    for (int i = 0; i < ADDON_COUNT; ++i) {
        if (i > 0) ImGui::SameLine();
        std::string label = std::string(ADDON_NAMES[i]) + " ^##pushall" + std::to_string(i);
        if (ImGui::Button(label.c_str())) {
            if (m_dirty) {
                m_pendingPushAddon   = i;
                m_confirmUnsavedPush = true;
            } else {
                openPushAll(i);
            }
        }
    }
    if (noChars) ImGui::EndDisabled();
}

// ─── Character table ──────────────────────────────────────────────────────────

void App::renderCharacterTable(float tableH)
{
    // Collect all unique accounts for the account dropdown.
    std::vector<std::string> allAccts;
    for (auto& [a, _] : m_scanned) allAccts.push_back(a);
    for (auto& c : m_cfg.characters)
        if (std::find(allAccts.begin(), allAccts.end(), c.account) == allAccts.end())
            allAccts.push_back(c.account);
    std::sort(allAccts.begin(), allAccts.end());

    static const char* factions[] = { "Alliance", "Horde" };

    // Subtract the Save/Remove row from the passed tableH.
    // When the table is empty show a help text placeholder instead of an empty table.
    float saveRowH = ImGui::GetFrameHeightWithSpacing();
    tableH -= saveRowH;
    if (tableH < 40.f) tableH = 40.f;

    if (m_cfg.characters.empty()) {
        // Placeholder: centered help text telling the user how to get started.
        ImGui::BeginChild("##emptyTable", ImVec2(0, tableH), true);
        const char* lines[] = {
            "To get started:",
            "1. Enter your WTF folder path above and click Apply.",
            "2. Select an account from the Accounts panel below.",
            "3. Select a character, then click Add.",
        };
        float totalH = ImGui::GetTextLineHeightWithSpacing() * 4
                     + ImGui::GetStyle().ItemSpacing.y;
        float startY = (tableH - totalH) * 0.5f;
        if (startY > 0.f) ImGui::SetCursorPosY(startY);
        for (auto& line : lines) {
            float textW = ImGui::CalcTextSize(line).x;
            float startX = (ImGui::GetContentRegionAvail().x - textW) * 0.5f;
            if (startX > 0.f) ImGui::SetCursorPosX(ImGui::GetCursorPosX() + startX);
            ImGui::TextUnformatted(line);
        }
        ImGui::EndChild();
    } else {

    ImGuiTableFlags flags =
        ImGuiTableFlags_Borders       |
        ImGuiTableFlags_Resizable     |
        ImGuiTableFlags_Sortable      |
        ImGuiTableFlags_ScrollY       |
        ImGuiTableFlags_RowBg         |
        ImGuiTableFlags_SizingStretchSame;

    if (ImGui::BeginTable("##chars", 6, flags, ImVec2(0, tableH))) {
        // Columns: [X] [^] Character Account Faction Enabled
        ImGui::TableSetupScrollFreeze(0, 1);
        ImGui::TableSetupColumn("##del",   ImGuiTableColumnFlags_NoSort | ImGuiTableColumnFlags_WidthFixed | ImGuiTableColumnFlags_NoResize, 26.f);
        ImGui::TableSetupColumn("##push",  ImGuiTableColumnFlags_NoSort | ImGuiTableColumnFlags_WidthFixed | ImGuiTableColumnFlags_NoResize, 26.f);
        ImGui::TableSetupColumn("Character", ImGuiTableColumnFlags_DefaultSort | ImGuiTableColumnFlags_WidthStretch);
        ImGui::TableSetupColumn("Account",   ImGuiTableColumnFlags_WidthStretch);
        ImGui::TableSetupColumn("Faction",   ImGuiTableColumnFlags_WidthStretch);
        ImGui::TableSetupColumn("Enabled",   ImGuiTableColumnFlags_WidthFixed, 60.f);
        ImGui::TableHeadersRow();

        // Handle sort request.
        if (ImGuiTableSortSpecs* ss = ImGui::TableGetSortSpecs()) {
            if (ss->SpecsDirty && !m_cfg.characters.empty()) {
                auto& spec = ss->Specs[0];
                int col = spec.ColumnIndex;
                bool asc = (spec.SortDirection == ImGuiSortDirection_Ascending);
                std::sort(m_cfg.characters.begin(), m_cfg.characters.end(),
                    [col, asc](const CharacterEntry& a, const CharacterEntry& b) {
                        std::string va, vb;
                        switch (col) {
                            case 2: va = a.characterName+"-"+a.realmName; vb = b.characterName+"-"+b.realmName; break;
                            case 3: va = a.account;  vb = b.account;  break;
                            case 4: va = a.faction;  vb = b.faction;  break;
                            case 5: va = a.enabled?"1":"0"; vb = b.enabled?"1":"0"; break;
                        }
                        return asc ? va < vb : va > vb;
                    });
                ss->SpecsDirty = false;
            }
        }

        int toDelete = -1;
        int pushRow  = -1;
        int pushAddon = -1;

        for (int row = 0; row < (int)m_cfg.characters.size(); ++row) {
            auto& c = m_cfg.characters[row];
            ImGui::TableNextRow();

            // [X] delete button
            ImGui::TableSetColumnIndex(0);
            ImGui::PushID(row * 10 + 0);
            if (ImGui::SmallButton("X"))
                m_confirmDeleteRow = row;
            ImGui::PopID();

            // [^] push-per-character context menu
            ImGui::TableSetColumnIndex(1);
            ImGui::PushID(row * 10 + 1);
            if (ImGui::SmallButton("^")) {
                ImGui::OpenPopup("##pushctx");
            }
            if (ImGui::BeginPopup("##pushctx")) {
                for (int ai = 0; ai < ADDON_COUNT; ++ai) {
                    if (ImGui::MenuItem(ADDON_NAMES[ai])) {
                        pushRow   = row;
                        pushAddon = ai;
                    }
                }
                ImGui::EndPopup();
            }
            ImGui::PopID();

            // Character name-realm
            ImGui::TableSetColumnIndex(2);
            ImGui::TextUnformatted((c.characterName + "-" + c.realmName).c_str());

            // Account dropdown
            ImGui::TableSetColumnIndex(3);
            ImGui::PushID(row * 10 + 3);
            ImGui::SetNextItemWidth(-FLT_MIN);
            int acctIdx = 0;
            for (int ai = 0; ai < (int)allAccts.size(); ++ai)
                if (allAccts[ai] == c.account) { acctIdx = ai; break; }
            if (ImGui::BeginCombo("##acct", c.account.c_str())) {
                for (int ai = 0; ai < (int)allAccts.size(); ++ai) {
                    bool sel = (ai == acctIdx);
                    if (ImGui::Selectable(allAccts[ai].c_str(), sel)) {
                        c.account = allAccts[ai];
                        checkDirty();
                    }
                }
                ImGui::EndCombo();
            }
            ImGui::PopID();

            // Faction dropdown
            ImGui::TableSetColumnIndex(4);
            ImGui::PushID(row * 10 + 4);
            ImGui::SetNextItemWidth(-FLT_MIN);
            int factIdx = (c.faction == "Horde") ? 1 : 0;
            if (ImGui::BeginCombo("##fact", factions[factIdx])) {
                for (int fi = 0; fi < 2; ++fi) {
                    bool sel = (fi == factIdx);
                    if (ImGui::Selectable(factions[fi], sel)) {
                        c.faction = factions[fi];
                        checkDirty();
                    }
                }
                ImGui::EndCombo();
            }
            ImGui::PopID();

            // Enabled checkbox
            ImGui::TableSetColumnIndex(5);
            ImGui::PushID(row * 10 + 5);
            if (ImGui::Checkbox("##en", &c.enabled))
                checkDirty();
            ImGui::PopID();
        }

        ImGui::EndTable();

        if (pushRow >= 0) openPushRow(pushRow, pushAddon);
        if (m_confirmDeleteRow >= 0 && toDelete < 0) {
            // Will be handled in renderModals()
        }
    }

    } // end else (characters non-empty)

    // ── Row below table: Remove All (left) + Save (right) ─────────────────
    {
        bool hasChars = !m_cfg.characters.empty();
        if (!hasChars) ImGui::BeginDisabled();
        if (ImGui::Button("X  Remove All"))
            m_confirmDeleteAll = true;
        if (!hasChars) ImGui::EndDisabled();

        ImGui::SameLine();
        float saveW = 80.f;
        ImGui::SetCursorPosX(
            ImGui::GetCursorPosX()
            + ImGui::GetContentRegionAvail().x - saveW);
        bool isDirty = m_dirty;
        if (!isDirty) ImGui::BeginDisabled();
        if (ImGui::Button("Save", ImVec2(saveW, 0)))
            saveConfig();
        if (!isDirty) ImGui::EndDisabled();
    }
}

// ─── Bottom scan panels ───────────────────────────────────────────────────────

void App::renderScanPanels(float panelH)
{
    float availW = ImGui::GetContentRegionAvail().x;
    // Subtract the Add button row so the panels don't overflow.
    panelH -= ImGui::GetFrameHeightWithSpacing();
    if (panelH < 30.f) panelH = 30.f;
    float halfW  = (availW - ImGui::GetStyle().ItemSpacing.x) * 0.5f;

    // Left panel: accounts
    ImGui::BeginChild("##accounts", ImVec2(halfW, panelH), true);
    ImGui::TextUnformatted("Accounts");
    ImGui::Separator();
    for (int i = 0; i < (int)m_accountList.size(); ++i) {
        bool sel = (i == m_selAccount);
        if (ImGui::Selectable(m_accountList[i].c_str(), sel)) {
            if (m_selAccount != i) { m_selAccount = i; m_selChar = -1; }
        }
    }
    ImGui::EndChild();

    ImGui::SameLine();

    // Right panel: characters under selected account
    ImGui::BeginChild("##charlist", ImVec2(halfW, panelH), true);
    ImGui::TextUnformatted("Characters");
    ImGui::Separator();
    if (m_selAccount >= 0 && m_selAccount < (int)m_accountList.size()) {
        auto& chars = m_scanned[m_accountList[m_selAccount]];
        for (int i = 0; i < (int)chars.size(); ++i) {
            bool sel = (i == m_selChar);
            if (ImGui::Selectable(chars[i].c_str(), sel))
                m_selChar = i;
        }
    }
    ImGui::EndChild();

    // Add button: right-justified, below the right panel
    float addW = 80.f;
    ImGui::SetCursorPosX(ImGui::GetCursorPosX() + availW - addW);
    bool canAdd = (m_selChar >= 0);
    if (!canAdd) ImGui::BeginDisabled();
    if (ImGui::Button("Add", ImVec2(addW, 0)))
        addCharacter();
    if (!canAdd) ImGui::EndDisabled();
}

// ─── Status bar ───────────────────────────────────────────────────────────────

void App::renderStatusBar()
{
    ImGui::Separator();
    ImGui::TextUnformatted(("Last push: " + m_statusText).c_str());
    if (m_pushDialog.state == PushState::Done) refreshStatus();
}

// ─── Confirm modals ───────────────────────────────────────────────────────────

void App::renderModals()
{
    // Delete single row
    if (m_confirmDeleteRow >= 0) {
        ImGui::OpenPopup("##confirmDel");
        ImVec2 c = ImGui::GetMainViewport()->GetCenter();
        ImGui::SetNextWindowPos(c, ImGuiCond_Always, ImVec2(0.5f,0.5f));
        if (ImGui::BeginPopupModal("##confirmDel", nullptr,
                ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_NoTitleBar))
        {
            auto& ch = m_cfg.characters[m_confirmDeleteRow];
            std::string msg = "Remove " + ch.characterName + "-" + ch.realmName + "?";
            ImGui::TextUnformatted(msg.c_str());
            ImGui::Spacing();
            if (ImGui::Button("Yes", ImVec2(80,0))) {
                Logger::log(LogKeyword::REMOVE,
                    ch.characterName + "-" + ch.realmName + " (" + ch.account + ")");
                m_cfg.characters.erase(m_cfg.characters.begin() + m_confirmDeleteRow);
                checkDirty();
                m_confirmDeleteRow = -1;
                ImGui::CloseCurrentPopup();
            }
            ImGui::SameLine();
            if (ImGui::Button("No", ImVec2(80,0))) {
                m_confirmDeleteRow = -1;
                ImGui::CloseCurrentPopup();
            }
            ImGui::EndPopup();
        }
    }

    // Delete all rows
    if (m_confirmDeleteAll) {
        ImGui::OpenPopup("##confirmDelAll");
        ImVec2 c = ImGui::GetMainViewport()->GetCenter();
        ImGui::SetNextWindowPos(c, ImGuiCond_Always, ImVec2(0.5f,0.5f));
        if (ImGui::BeginPopupModal("##confirmDelAll", nullptr,
                ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_NoTitleBar))
        {
            ImGui::TextUnformatted("Remove all characters from the table?");
            ImGui::Spacing();
            if (ImGui::Button("Yes", ImVec2(80,0))) {
                Logger::log(LogKeyword::REMOVE, "All characters removed.");
                m_cfg.characters.clear();
                checkDirty();
                m_confirmDeleteAll = false;
                ImGui::CloseCurrentPopup();
            }
            ImGui::SameLine();
            if (ImGui::Button("No", ImVec2(80,0))) {
                m_confirmDeleteAll = false;
                ImGui::CloseCurrentPopup();
            }
            ImGui::EndPopup();
        }
    }

    // Unsaved changes — exit
    if (m_confirmUnsavedExit) {
        ImGui::OpenPopup("##confirmUnsavedExit");
        ImVec2 c = ImGui::GetMainViewport()->GetCenter();
        ImGui::SetNextWindowPos(c, ImGuiCond_Always, ImVec2(0.5f, 0.5f));
        if (ImGui::BeginPopupModal("##confirmUnsavedExit", nullptr,
                ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_NoTitleBar))
        {
            ImGui::TextUnformatted("You have unsaved changes. Save before exiting?");
            ImGui::Spacing();
            if (ImGui::Button("Save & Exit", ImVec2(110, 0))) {
                saveConfig();
                m_quit = true;
                m_confirmUnsavedExit = false;
                ImGui::CloseCurrentPopup();
            }
            ImGui::SameLine();
            if (ImGui::Button("Exit without Saving", ImVec2(150, 0))) {
                m_quit = true;
                m_confirmUnsavedExit = false;
                ImGui::CloseCurrentPopup();
            }
            ImGui::SameLine();
            if (ImGui::Button("Cancel", ImVec2(80, 0))) {
                m_confirmUnsavedExit = false;
                ImGui::CloseCurrentPopup();
            }
            ImGui::EndPopup();
        }
    }

    // Duplicate character notification
    if (m_notifyDuplicate) {
        ImGui::OpenPopup("##notifyDup");
        ImVec2 c = ImGui::GetMainViewport()->GetCenter();
        ImGui::SetNextWindowPos(c, ImGuiCond_Always, ImVec2(0.5f, 0.5f));
        if (ImGui::BeginPopupModal("##notifyDup", nullptr,
                ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_NoTitleBar))
        {
            ImGui::TextUnformatted("That character has already been added.");
            ImGui::Spacing();
            if (ImGui::Button("OK", ImVec2(80, 0))) {
                m_notifyDuplicate = false;
                ImGui::CloseCurrentPopup();
            }
            ImGui::EndPopup();
        }
    }

    // Unsaved changes — push all
    if (m_confirmUnsavedPush) {
        ImGui::OpenPopup("##confirmUnsavedPush");
        ImVec2 c = ImGui::GetMainViewport()->GetCenter();
        ImGui::SetNextWindowPos(c, ImGuiCond_Always, ImVec2(0.5f, 0.5f));
        if (ImGui::BeginPopupModal("##confirmUnsavedPush", nullptr,
                ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_NoTitleBar))
        {
            ImGui::TextUnformatted("You have unsaved changes. Save before pushing?");
            ImGui::Spacing();
            if (ImGui::Button("Save & Push", ImVec2(110, 0))) {
                saveConfig();
                openPushAll(m_pendingPushAddon);
                m_pendingPushAddon  = -1;
                m_confirmUnsavedPush = false;
                ImGui::CloseCurrentPopup();
            }
            ImGui::SameLine();
            if (ImGui::Button("Cancel", ImVec2(80, 0))) {
                m_pendingPushAddon  = -1;
                m_confirmUnsavedPush = false;
                ImGui::CloseCurrentPopup();
            }
            ImGui::EndPopup();
        }
    }
}
