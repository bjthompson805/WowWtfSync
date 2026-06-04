#pragma once
#include <QString>
#include <QStringList>
#include <QMap>

/// Scans a WTF folder for Battle.net accounts and the characters within them.
class WtfScanner {
public:
    /// Returns the list of account directory names (matching \d{8}#\d+)
    /// found directly inside wtfPath.
    static QStringList findAccounts(const QString& wtfPath);

    /// Returns all characters for every account.
    /// Key = account name, Value = list of "CharName-Realm" strings.
    static QMap<QString, QStringList> findAllCharacters(const QString& wtfPath);

    /// Returns characters for a single account as "CharName-Realm" strings.
    static QStringList findCharactersForAccount(const QString& wtfPath,
                                                const QString& account);
};
