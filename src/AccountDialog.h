#pragma once
#include "imgui.h"
#include "AuthSession.h"
#include "UrlHelper.h"

// "My Account" modal — opened from the menu bar.
struct AccountDialog {
    bool  open     = false;
    float dpiScale = 1.0f;

    void show(AuthSession& auth)
    {
        open = true;
        auth.refreshMe();  // reload latest info from the server
    }

    // Returns true when the user dismisses the dialog.
    bool render(const AuthSession& auth)
    {
        if (!open) return false;

        ImGui::OpenPopup("My Account##acctdlg");
        ImVec2 center = ImGui::GetMainViewport()->GetCenter();
        ImGui::SetNextWindowPos(center, ImGuiCond_Always, ImVec2(0.5f, 0.5f));
        ImGui::SetNextWindowSize(ImVec2(420 * dpiScale, 0), ImGuiCond_Always);

        bool dismissed = false;
        if (ImGui::BeginPopupModal("My Account##acctdlg", &open,
                ImGuiWindowFlags_AlwaysAutoResize))
        {
            const UserInfo& u = auth.userInfo();

            ImGui::Spacing();
            renderRow("Username",     u.username.empty()  ? "N/A" : u.username.c_str());
            renderRow("Email",        u.email.empty()     ? "N/A" : u.email.c_str());

            // Subscription status with colour coding
            const char* status = u.subStatus.empty() ? "N/A" : u.subStatus.c_str();
            ImGui::TextUnformatted("Subscription:");
            ImGui::SameLine(130.f * dpiScale);
            if (u.subStatus == "active")
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.3f, 1.f, 0.4f, 1.f));
            else
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.f, 0.4f, 0.4f, 1.f));
            ImGui::TextUnformatted(status);
            ImGui::PopStyleColor();

            if (!u.currentPeriodEnd.empty())
                renderRow("Renews / Expires", u.currentPeriodEnd.c_str());

            renderRow("License Key",
                u.licenseKey.empty() ? "N/A" : u.licenseKey.c_str());

            ImGui::Spacing();
            ImGui::Separator();
            ImGui::Spacing();

            // Dashboard link
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.35f, 0.65f, 1.f, 1.f));
            ImGui::TextUnformatted("Open Dashboard");
            ImGui::PopStyleColor();
            if (ImGui::IsItemHovered()) ImGui::SetMouseCursor(ImGuiMouseCursor_Hand);
            if (ImGui::IsItemClicked()) openUrl(URL_DASHBOARD);

            ImGui::SameLine();
            float okW = 60.f * dpiScale;
            ImGui::SetCursorPosX(
                ImGui::GetCursorPosX()
                + ImGui::GetContentRegionAvail().x - okW);
            if (ImGui::Button("OK", ImVec2(okW, 0))) {
                dismissed = true;
                open = false;
                ImGui::CloseCurrentPopup();
            }

            ImGui::Spacing();
            ImGui::EndPopup();
        }

        if (!open) dismissed = true;
        return dismissed;
    }

private:
    void renderRow(const char* label, const char* value) const
    {
        ImGui::TextUnformatted(label);
        ImGui::SameLine(130.f * dpiScale);
        ImGui::TextUnformatted(value);
    }
};
