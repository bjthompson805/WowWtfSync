#pragma once
#include <QString>
#include <QJsonArray>
#include <QJsonObject>

/// Manages the application's JSON configuration file.
/// Config is stored in the OS-standard app data location.
class ConfigManager {
public:
    static ConfigManager& instance();

    void load();
    void save();

    QString wtfPath() const;
    void    setWtfPath(const QString& path);

    QJsonArray  characters() const;
    void        setCharacters(const QJsonArray& chars);

private:
    ConfigManager();
    QString     configFilePath() const;

    QJsonObject m_config;
};
