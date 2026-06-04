#pragma once
#include <QString>

/// Append-only logger that tags each entry with a keyword.
/// Keywords let callers retrieve the most recent entry of a specific type
/// (e.g. the last PUSH event for the status bar).
class Logger {
public:
    enum Keyword {
        PUSH,       ///< A push operation was performed
        CONFIG,     ///< WTF path or config changed
        CHARACTER,  ///< A character was added / removed
        ACTION,     ///< Other user-initiated action
        GENERAL     ///< Miscellaneous
    };

    static Logger& instance();

    /// Append a line to the log: "[YYYY-MM-DD HH:MM:SS] [KEYWORD] message"
    void    log(Keyword kw, const QString& message);

    /// Return the message text of the most recent entry that has the given keyword.
    /// Returns an empty string if no such entry exists.
    QString lastEntry(Keyword kw) const;

    QString logFilePath() const;

private:
    Logger();
    QString keywordStr(Keyword kw) const;
};
