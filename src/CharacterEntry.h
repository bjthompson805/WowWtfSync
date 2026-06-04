#pragma once
#include <QString>

struct CharacterEntry {
    QString characterName;   // e.g. "Thrall"
    QString realmName;       // e.g. "Grobbulus"
    QString account;         // e.g. "12345678#1"
    QString faction;         // "Alliance" or "Horde"
    bool    enabled = true;

    /// Display string used throughout the UI: "CharName-Realm"
    QString displayName() const { return characterName + "-" + realmName; }
};
