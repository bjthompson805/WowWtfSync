#pragma once
#include <QDialog>
#include <QString>

class QLabel;
class QPushButton;
class QTextEdit;
class QProcess;

/// Dialog shown when the user initiates a push operation (either "Push All"
/// or the per-character push context menu).
///
/// It displays a description of what will happen, then on confirmation runs
///   lua push.lua <args>
/// and shows the result.
class PushDialog : public QDialog {
    Q_OBJECT
public:
    /// \param addon        One of: Altoholic, Auctionator, Bagnon,
    ///                              NovaWorldBuffs, TitanGold
    /// \param characterDisplay  "CharName-Realm" or empty string for "All Characters"
    /// \param account      Source account (empty when characterDisplay is empty)
    /// \param wtfPath      Absolute path to the WTF folder
    /// \param allAccounts  Full list of account directory names found in WTF
    explicit PushDialog(const QString& addon,
                        const QString& characterDisplay,
                        const QString& account,
                        const QString& wtfPath,
                        const QStringList& allAccounts,
                        QWidget* parent = nullptr);

private slots:
    void onPushClicked();
    void onProcessFinished(int exitCode);

private:
    void buildUi();
    void showResult(bool success, const QString& message);
    QString buildDescription() const;
    QStringList buildLuaArgs() const;
    QString findLuaExecutable() const;

    QString     m_addon;
    QString     m_characterDisplay;  ///< "CharName-Realm" or ""
    QString     m_account;
    QString     m_wtfPath;
    QStringList m_allAccounts;

    QLabel*      m_headerLabel  = nullptr;
    QLabel*      m_descLabel    = nullptr;
    QPushButton* m_pushBtn      = nullptr;
    QTextEdit*   m_outputEdit   = nullptr;
    QPushButton* m_okBtn        = nullptr;
    QProcess*    m_process      = nullptr;
};
