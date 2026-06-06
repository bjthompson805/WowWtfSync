#pragma once
#include <map>
#include <string>
#include <vector>
#include "AuthSession.h"
#include "AccountDialog.h"
#include "LoginScreen.h"
#include "Config.h"
#include "FolderPicker.h"
#include "PushDialog.h"

struct SDL_Window;
struct SDL_Renderer;

class App {
public:
    App(SDL_Window* window, SDL_Renderer* renderer);
    void render();

    void requestQuit();
    bool shouldQuit() const { return m_quit; }

private:
    // ── Auth ───────────────────────────────────────────────────────────────
    AuthSession   m_auth;
    LoginScreen   m_loginScreen;
    AccountDialog m_accountDialog;

    void renderCheckingScreen();
    void renderNoSubscriptionScreen();
    void renderMenuBar();
    void renderMainApp();    // the full app UI (table, panels, etc.)

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

    std::map<std::string, std::vector<std::string>> m_scanned;
    std::vector<std::string> m_accountList;
    int  m_selAccount = -1;
    int  m_selChar    = -1;

    // ── Dialogs ────────────────────────────────────────────────────────────
    PushDialog   m_pushDialog;
    FolderPicker m_folderPicker;

    int  m_pendingPushAddon  = -1;
    bool m_confirmDeleteAll      = false;
    int  m_confirmDeleteRow      = -1;
    bool m_confirmSignOut        = false;   // simple "are you sure?" sign-out
    bool m_confirmUnsavedExit    = false;
    bool m_confirmUnsavedSignOut = false;   // sign-out with unsaved changes
    bool m_confirmUnsavedPush    = false;
    bool m_notifyDuplicate       = false;

    // ── Status bar ─────────────────────────────────────────────────────────
    std::string m_statusText;
    void refreshStatus();

    // ── Helpers ────────────────────────────────────────────────────────────
    void applyWtfPath();
    void saveConfig();
    void addCharacter();
    void openPushAll(int addonIdx);
    void openPushRow(int row, int addonIdx);

    void renderWtfBar();
    void renderPushAllBar();
    void renderCharacterTable(float tableH);
    void renderScanPanels(float panelH);
    void renderStatusBar();
    void renderModals();

    std::vector<std::string> tableAccounts() const;
};
