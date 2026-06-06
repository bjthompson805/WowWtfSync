#pragma once
#include <map>
#include <string>
#include <vector>
#include "Config.h"
#include "FolderPicker.h"
#include "PushDialog.h"

struct SDL_Window;
struct SDL_Renderer;

class App {
public:
    App(SDL_Window* window, SDL_Renderer* renderer);
    void render();

    // Called by main() when the OS/user requests a close.
    // If there are unsaved changes a modal is shown; otherwise quit is immediate.
    void requestQuit();

    // main() checks this every frame to know when it is safe to exit.
    bool shouldQuit() const { return m_quit; }

private:
    // ── Paths ──────────────────────────────────────────────────────────────
    std::string m_configPath;
    std::string m_pushLuaPath;
    std::string m_logPath;
    std::string m_luaInterp;

    // ── State ──────────────────────────────────────────────────────────────
    AppConfig   m_cfg;
    AppConfig   m_savedCfg;
    bool        m_dirty = false;
    bool        m_quit  = false;

    void checkDirty() { m_dirty = !(m_cfg == m_savedCfg); }

    char m_wtfBuf[1024] = {};

    // WTF scan results: account → list of "CharName-Realm"
    std::map<std::string, std::vector<std::string>> m_scanned;
    std::vector<std::string> m_accountList;
    int  m_selAccount = -1;
    int  m_selChar    = -1;

    // ── Dialogs ────────────────────────────────────────────────────────────
    PushDialog   m_pushDialog;
    FolderPicker m_folderPicker;

    // Pending addon index for a Push All that was deferred by the unsaved-
    // changes modal (-1 = no deferred push).
    int  m_pendingPushAddon = -1;

    // Modal flags
    bool m_confirmDeleteAll   = false;
    int  m_confirmDeleteRow   = -1;
    bool m_confirmUnsavedExit = false;   // exit with unsaved changes?
    bool m_confirmUnsavedPush = false;   // push all with unsaved changes?
    bool m_notifyDuplicate    = false;   // attempted to add a duplicate character

    // ── Status bar ─────────────────────────────────────────────────────────
    std::string m_statusText;
    void refreshStatus();

    // ── Helpers ────────────────────────────────────────────────────────────
    void applyWtfPath();
    void saveConfig();
    void addCharacter();
    void openPushAll(int addonIdx);
    void openPushRow(int row, int addonIdx);

    // UI sections
    void renderWtfBar();
    void renderPushAllBar();
    void renderCharacterTable(float tableH);
    void renderScanPanels(float panelH);
    void renderStatusBar();
    void renderModals();

    std::vector<std::string> tableAccounts() const;
};
