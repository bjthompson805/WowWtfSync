#pragma once
#include "imgui.h"
#include <filesystem>
#include <string>
#include <vector>
#include <algorithm>

namespace fs = std::filesystem;

// Simple ImGui-based folder browser.
// Call show() to open, then call render() every frame.
// When render() returns true the user confirmed; read `selected`.
struct FolderPicker {
    bool        open     = false;
    std::string selected;           // set when user confirms

    void show(const std::string& startPath)
    {
        fs::path p(startPath);
        m_current = (fs::is_directory(p) ? p : p.parent_path()).string();
        if (m_current.empty()) m_current = fs::current_path().string();
        m_selIdx  = -1;
        selected.clear();
        open = true;
    }

    // Returns true when the user has confirmed a selection.
    bool render()
    {
        if (!open) return false;
        ImGui::OpenPopup("Choose Folder");

        ImVec2 center = ImGui::GetMainViewport()->GetCenter();
        ImGui::SetNextWindowPos(center, ImGuiCond_Always, ImVec2(0.5f, 0.5f));
        ImGui::SetNextWindowSize(ImVec2(540, 380), ImGuiCond_Always);

        bool confirmed = false;
        if (ImGui::BeginPopupModal("Choose Folder", nullptr,
                ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoMove))
        {
            // Current path display
            ImGui::TextUnformatted("Location:");
            ImGui::SameLine();
            ImGui::TextUnformatted(m_current.c_str());
            ImGui::Separator();

            // Directory list
            ImGui::BeginChild("##dirlist", ImVec2(0, -ImGui::GetFrameHeightWithSpacing() * 2 - 8), true);

            // ".." to go up
            bool hasParent = fs::path(m_current).has_parent_path() &&
                             fs::path(m_current).parent_path() != fs::path(m_current);
            if (hasParent) {
                if (ImGui::Selectable("..", false, ImGuiSelectableFlags_AllowDoubleClick)) {
                    if (ImGui::IsMouseDoubleClicked(0))
                        navigate(fs::path(m_current).parent_path().string());
                }
            }

            // Subdirectories
            refreshIfNeeded();
            for (int i = 0; i < (int)m_entries.size(); ++i) {
                bool sel = (i == m_selIdx);
                if (ImGui::Selectable(m_entries[i].c_str(), sel,
                        ImGuiSelectableFlags_AllowDoubleClick)) {
                    m_selIdx = i;
                    if (ImGui::IsMouseDoubleClicked(0)) {
                        navigate((fs::path(m_current) / m_entries[i]).string());
                    }
                }
            }
            ImGui::EndChild();

            // Buttons
            float btnW = 140.f;
            if (ImGui::Button("Select This Folder", ImVec2(btnW, 0))) {
                selected = (m_selIdx >= 0)
                    ? (fs::path(m_current) / m_entries[m_selIdx]).string()
                    : m_current;
                confirmed = true;
                open      = false;
                ImGui::CloseCurrentPopup();
            }
            ImGui::SameLine();
            if (ImGui::Button("Cancel", ImVec2(80, 0))) {
                open = false;
                ImGui::CloseCurrentPopup();
            }

            ImGui::EndPopup();
        }
        return confirmed;
    }

private:
    std::string              m_current;
    std::string              m_lastListed;
    std::vector<std::string> m_entries;
    int                      m_selIdx = -1;

    void navigate(const std::string& path)
    {
        m_current = path;
        m_selIdx  = -1;
        m_entries.clear();
        m_lastListed.clear(); // force refresh
    }

    void refreshIfNeeded()
    {
        if (m_current == m_lastListed) return;
        m_entries.clear();
        try {
            for (auto& e : fs::directory_iterator(m_current,
                    fs::directory_options::skip_permission_denied)) {
                if (e.is_directory())
                    m_entries.push_back(e.path().filename().string());
            }
        } catch (...) {}
        std::sort(m_entries.begin(), m_entries.end());
        m_lastListed = m_current;
    }
};
