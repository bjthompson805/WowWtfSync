#include "Logger.h"

#include <QFile>
#include <QDir>
#include <QCoreApplication>
#include <QDateTime>
#include <QTextStream>
#include <QStandardPaths>

Logger& Logger::instance() {
    static Logger inst;
    return inst;
}

Logger::Logger() {}

QString Logger::logFilePath() const {
    // Use the OS-standard writable app-data directory so this works correctly
    // inside read-only environments such as AppImage mounts.
    // Linux:   ~/.local/share/WowWtfSync/WowWtfSync/
    // macOS:   ~/Library/Application Support/WowWtfSync/
    // Windows: %APPDATA%\WowWtfSync\WowWtfSync
    QString dir = QStandardPaths::writableLocation(QStandardPaths::AppDataLocation);
    QDir().mkpath(dir);   // create the directory if it doesn't exist yet
    return dir + "/log.txt";
}

QString Logger::keywordStr(Keyword kw) const {
    switch (kw) {
        case PUSH:      return "PUSH";
        case CONFIG:    return "CONFIG";
        case CHARACTER: return "CHARACTER";
        case ACTION:    return "ACTION";
        case GENERAL:   return "GENERAL";
    }
    return "GENERAL";
}

void Logger::log(Keyword kw, const QString& message) {
    QFile f(logFilePath());
    if (!f.open(QIODevice::Append | QIODevice::Text)) return;
    QTextStream out(&f);
    QString ts = QDateTime::currentDateTime().toString("yyyy-MM-dd HH:mm:ss");
    out << "[" << ts << "] [" << keywordStr(kw) << "] " << message << "\n";
}

QString Logger::lastEntry(Keyword kw) const {
    QFile f(logFilePath());
    if (!f.open(QIODevice::ReadOnly | QIODevice::Text)) return {};

    QString tag     = "[" + keywordStr(kw) + "]";
    QString lastMsg;
    QTextStream in(&f);
    while (!in.atEnd()) {
        QString line = in.readLine();
        if (line.contains(tag)) {
            // Extract timestamp from first [...] bracket
            QString timestamp;
            if (line.startsWith("[")) {
                int tsEnd = line.indexOf(']');
                if (tsEnd > 0)
                    timestamp = line.mid(1, tsEnd - 1);
            }
            // Extract the message part after the keyword tag
            int pos = line.indexOf(tag);
            if (pos >= 0) {
                pos += tag.length();
                QString msg = line.mid(pos).trimmed();
                lastMsg = timestamp.isEmpty() ? msg : timestamp + " " + msg;
            }
        }
    }
    return lastMsg;
}
