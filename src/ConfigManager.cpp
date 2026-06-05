#include "ConfigManager.h"

#include <QFile>
#include <QDir>
#include <QCoreApplication>
#include <QJsonDocument>
#include <QStandardPaths>

ConfigManager& ConfigManager::instance() {
    static ConfigManager inst;
    return inst;
}

ConfigManager::ConfigManager() {
    load();
}

QString ConfigManager::configFilePath() const {
    // Use the OS-standard writable app-data directory so this works correctly
    // inside read-only environments such as AppImage mounts.
    // Linux:   ~/.local/share/WowWtfSync/WowWtfSync/
    // macOS:   ~/Library/Application Support/WowWtfSync/
    // Windows: %APPDATA%\WowWtfSync\WowWtfSync
    QString dir = QStandardPaths::writableLocation(QStandardPaths::AppDataLocation);
    QDir().mkpath(dir);   // create the directory if it doesn't exist yet
    return dir + "/config.json";
}

void ConfigManager::load() {
    QFile f(configFilePath());
    if (!f.open(QIODevice::ReadOnly)) return;
    QJsonDocument doc = QJsonDocument::fromJson(f.readAll());
    if (doc.isObject()) m_config = doc.object();
}

void ConfigManager::save() {
    QFile f(configFilePath());
    if (!f.open(QIODevice::WriteOnly | QIODevice::Truncate)) return;
    f.write(QJsonDocument(m_config).toJson(QJsonDocument::Indented));
}

QString ConfigManager::wtfPath() const {
    return m_config.value("wtfPath").toString();
}

void ConfigManager::setWtfPath(const QString& path) {
    m_config["wtfPath"] = path;
}

QJsonArray ConfigManager::characters() const {
    return m_config.value("characters").toArray();
}

void ConfigManager::setCharacters(const QJsonArray& chars) {
    m_config["characters"] = chars;
}
