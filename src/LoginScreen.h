#pragma once
#include "imgui.h"
#include "AuthSession.h"
#include "UrlHelper.h"
#include <cstring>

// Renders the centered login panel.  Call every frame when AuthState == LoggedOut.
struct LoginScreen {
    char  emailBuf[256]    = {};
    char  passwordBuf[256] = {};
    bool  showForgot       = false;
    char  forgotEmailBuf[256] = {};

    void render(AuthSession& auth)
    {
        // Full-screen dim background
        ImGuiViewport* vp = ImGui::GetMainViewport();
        ImGui::SetNextWindowPos(vp->Pos);
        ImGui::SetNextWindowSize(vp->Size);
        ImGui::SetNextWindowBgAlpha(1.f);
        ImGui::Begin("##loginBg", nullptr,
            ImGuiWindowFlags_NoDecoration |
            ImGuiWindowFlags_NoMove       |
            ImGuiWindowFlags_NoSavedSettings);

        if (!showForgot)
            renderLogin(auth);
        else
            renderForgotPassword(auth);

        ImGui::End();
    }

private:
    // ── Helpers ──────────────────────────────────────────────────────────────
    static void linkText(const char* label)
    {
        ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.35f, 0.65f, 1.f, 1.f));
        ImGui::TextUnformatted(label);
        ImGui::PopStyleColor();
        if (ImGui::IsItemHovered())
            ImGui::SetMouseCursor(ImGuiMouseCursor_Hand);
    }

    static void centeredText(const char* text)
    {
        float w = ImGui::CalcTextSize(text).x;
        ImGui::SetCursorPosX((ImGui::GetContentRegionAvail().x - w) * 0.5f
                             + ImGui::GetCursorPosX());
        ImGui::TextUnformatted(text);
    }

    // ── Login panel ──────────────────────────────────────────────────────────
    void renderLogin(AuthSession& auth)
    {
        ImGuiViewport* vp = ImGui::GetMainViewport();
        const float panelW = 380.f;
        const float panelH = 290.f;
        ImGui::SetNextWindowPos(
            ImVec2(vp->Pos.x + (vp->Size.x - panelW) * 0.5f,
                   vp->Pos.y + (vp->Size.y - panelH) * 0.5f));
        ImGui::SetNextWindowSize(ImVec2(panelW, panelH));
        ImGui::SetNextWindowBgAlpha(0.96f);
        ImGui::Begin("##loginPanel", nullptr,
            ImGuiWindowFlags_NoDecoration  |
            ImGuiWindowFlags_NoMove        |
            ImGuiWindowFlags_NoSavedSettings);

        ImGui::Spacing();
        centeredText("WoW WTF Sync");
        ImGui::Spacing();
        ImGui::Separator();
        ImGui::Spacing();

        bool busy = auth.isBusy();
        if (busy) ImGui::BeginDisabled();

        // Email
        ImGui::SetNextItemWidth(-FLT_MIN);
        ImGui::InputTextWithHint("##email", "Email", emailBuf, sizeof(emailBuf));

        // Password
        ImGui::SetNextItemWidth(-FLT_MIN);
        bool enter = ImGui::InputTextWithHint("##pw", "Password",
            passwordBuf, sizeof(passwordBuf),
            ImGuiInputTextFlags_Password | ImGuiInputTextFlags_EnterReturnsTrue);

        ImGui::Spacing();

        // Login button — full width
        bool doLogin = ImGui::Button("Log In", ImVec2(-FLT_MIN, 0)) || enter;
        if (busy) ImGui::EndDisabled();

        if (doLogin && !busy)
            auth.attemptLogin(emailBuf, passwordBuf);

        // Spinner while logging in
        if (busy) {
            static const char* frames[] = {"|", "/", "-", "\\"};
            ImGui::SameLine();
            ImGui::TextUnformatted(frames[(int)(ImGui::GetTime() * 8) % 4]);
        }

        ImGui::Spacing();

        // Error message
        const std::string& err = auth.error();
        if (!err.empty()) {
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.f, 0.35f, 0.35f, 1.f));
            ImGui::TextWrapped("%s", err.c_str());
            ImGui::PopStyleColor();
        }

        ImGui::Spacing();
        ImGui::Separator();
        ImGui::Spacing();

        // Links row
        linkText("Forgot password?");
        if (ImGui::IsItemClicked()) { showForgot = true; std::memset(forgotEmailBuf, 0, sizeof(forgotEmailBuf)); }

        ImGui::SameLine();
        float createW = ImGui::CalcTextSize("Create an account").x;
        ImGui::SetCursorPosX(ImGui::GetContentRegionAvail().x - createW
                             + ImGui::GetCursorPosX());
        linkText("Create an account");
        if (ImGui::IsItemClicked()) openUrl(URL_REGISTER);

        ImGui::End();
    }

    // ── Forgot-password panel ─────────────────────────────────────────────────
    void renderForgotPassword(AuthSession& auth)
    {
        ImGuiViewport* vp = ImGui::GetMainViewport();
        const float panelW = 380.f;
        const float panelH = 230.f;
        ImGui::SetNextWindowPos(
            ImVec2(vp->Pos.x + (vp->Size.x - panelW) * 0.5f,
                   vp->Pos.y + (vp->Size.y - panelH) * 0.5f));
        ImGui::SetNextWindowSize(ImVec2(panelW, panelH));
        ImGui::SetNextWindowBgAlpha(0.96f);
        ImGui::Begin("##forgotPanel", nullptr,
            ImGuiWindowFlags_NoDecoration  |
            ImGuiWindowFlags_NoMove        |
            ImGuiWindowFlags_NoSavedSettings);

        ImGui::Spacing();
        centeredText("Reset Password");
        ImGui::Spacing();
        ImGui::Separator();
        ImGui::Spacing();
        ImGui::TextWrapped("Enter your email and we'll send a reset link.");
        ImGui::Spacing();

        bool busy = auth.isBusy();
        if (busy) ImGui::BeginDisabled();

        ImGui::SetNextItemWidth(-FLT_MIN);
        bool enter = ImGui::InputTextWithHint("##femail", "Email",
            forgotEmailBuf, sizeof(forgotEmailBuf),
            ImGuiInputTextFlags_EnterReturnsTrue);

        ImGui::Spacing();
        bool send = ImGui::Button("Send Reset Link", ImVec2(-FLT_MIN, 0)) || enter;
        if (busy) ImGui::EndDisabled();

        if (send && !busy && forgotEmailBuf[0])
            auth.requestForgotPassword(forgotEmailBuf);

        // Result message
        const std::string& msg = auth.forgotPasswordMessage();
        if (!msg.empty()) {
            ImGui::Spacing();
            ImGui::TextWrapped("%s", msg.c_str());
        }

        ImGui::Spacing();
        ImGui::Separator();
        ImGui::Spacing();
        linkText("Back to login");
        if (ImGui::IsItemClicked()) showForgot = false;

        ImGui::End();
    }
};
