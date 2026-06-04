#include "WtfScanner.h"

#include <QDir>
#include <QRegularExpression>

static const QRegularExpression kAccountRx("^\\d{8}#\\d+$");

// Battle.net account folders live in WTF/Account/, not directly in WTF/
static QString accountsRoot(const QString& wtfPath) {
    return wtfPath + "/Account";
}

QStringList WtfScanner::findAccounts(const QString& wtfPath) {
    QStringList accounts;
    QDir dir(accountsRoot(wtfPath));
    if (!dir.exists()) return accounts;

    const auto entries = dir.entryList(QDir::Dirs | QDir::NoDotAndDotDot);
    for (const QString& name : entries) {
        if (kAccountRx.match(name).hasMatch()) {
            accounts << name;
        }
    }
    accounts.sort();
    return accounts;
}

QStringList WtfScanner::findCharactersForAccount(const QString& wtfPath,
                                                 const QString& account) {
    QStringList characters;

    QDir accountDir(accountsRoot(wtfPath) + "/" + account);
    if (!accountDir.exists()) return characters;

    // Each sub-directory that is NOT "SavedVariables" is a realm directory
    const auto realmDirs = accountDir.entryList(QDir::Dirs | QDir::NoDotAndDotDot);
    for (const QString& realm : realmDirs) {
        if (realm.compare("SavedVariables", Qt::CaseInsensitive) == 0) continue;

        QDir realmDir(accountDir.filePath(realm));
        const auto charDirs = realmDir.entryList(QDir::Dirs | QDir::NoDotAndDotDot);
        for (const QString& charName : charDirs) {
            characters << charName + "-" + realm;
        }
    }

    characters.sort();
    return characters;
}

QMap<QString, QStringList> WtfScanner::findAllCharacters(const QString& wtfPath) {
    QMap<QString, QStringList> result;
    const QStringList accounts = findAccounts(wtfPath);
    for (const QString& acc : accounts) {
        result[acc] = findCharactersForAccount(wtfPath, acc);
    }
    return result;
}
