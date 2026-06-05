#include "PushDialog.h"
#include "Logger.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QLabel>
#include <QPushButton>
#include <QTextEdit>
#include <QProcess>
#include <QFont>
#include <QMessageBox>
#include <QCoreApplication>
#include <QDir>
#include <QFileInfo>
#include <QSizePolicy>
#include <QStandardPaths>

// ─── helpers ─────────────────────────────────────────────────────────────────

static QString addonDescription(const QString& addon,
                                const QString& charDisplay,   // empty = all
                                const QString& account)
{
    bool isAll = charDisplay.isEmpty();
    QString who = isAll ? "each character" : charDisplay + "-" + account;

    if (addon == "Altoholic") {
        if (isAll)
            return "Push Altoholic data from all accounts, combining the data.";
        else
            return QString("Push Altoholic data for %1 by combining the data to/from all accounts.").arg(who);
    }

    QString base;
    if (isAll)
        base = QString("Push %1 data from each character to all accounts that are not the same "
                       "as the character's account.").arg(addon);
    else
        base = QString("Push %1 data from %2 to all accounts that are not the same "
                       "as the character's account.").arg(addon, who);

    QString detail;
    if (addon == "Auctionator")
        detail = " This will copy any scan days for an item that haven't been seen by the "
                 "destination account so that missing days will be merged in. "
                 "This process may take a minute.";
    else if (addon == "Bagnon")
        detail = " This will allow you to view characters' bags/banks that are on other accounts.";
    else if (addon == "NovaWorldBuffs")
        detail = " This will allow you to view characters' world buffs and raid lockouts that are on other accounts.";
    else if (addon == "TitanGold")
        detail = " This will allow you to view characters' gold that are on other accounts.";

    return base + detail;
}

// ─── PushDialog ──────────────────────────────────────────────────────────────

PushDialog::PushDialog(const QString& addon,
                       const QString& characterDisplay,
                       const QString& account,
                       const QString& wtfPath,
                       const QStringList& allAccounts,
                       QWidget* parent)
    : QDialog(parent)
    , m_addon(addon)
    , m_characterDisplay(characterDisplay)
    , m_account(account)
    , m_wtfPath(wtfPath)
    , m_allAccounts(allAccounts)
{
    setWindowTitle("Push Configuration");
    setModal(true);
    setMinimumWidth(540);
    buildUi();
}

void PushDialog::buildUi() {
    auto* root = new QVBoxLayout(this);
    root->setSpacing(12);
    root->setContentsMargins(18, 18, 18, 18);

    // ── Large header ──────────────────────────────────────────────────────
    QString who = m_characterDisplay.isEmpty() ? "All Characters" : m_characterDisplay;
    m_headerLabel = new QLabel(QString("Push %1  —  %2").arg(m_addon, who));
    QFont hf = m_headerLabel->font();
    hf.setPointSize(hf.pointSize() + 6);
    hf.setBold(true);
    m_headerLabel->setFont(hf);
    m_headerLabel->setWordWrap(true);
    root->addWidget(m_headerLabel);

    // ── Description ───────────────────────────────────────────────────────
    m_descLabel = new QLabel();
    m_descLabel->setWordWrap(true);

    QString desc = addonDescription(m_addon, m_characterDisplay, m_account);
    // Logout warning (always)
    desc += "\n\nMake sure to log out of all characters that are being pushed to, "
            "otherwise the data will be overwritten when logout occurs.";
    // Altoholic mail note
    if (m_addon == "Altoholic")
        desc += "\n\n*Aggregation of mail sent to your own character from another account is also supported.";

    m_descLabel->setText(desc);
    root->addWidget(m_descLabel);

    root->addStretch();

    // ── Output area (hidden until push runs) ─────────────────────────────
    m_outputEdit = new QTextEdit();
    m_outputEdit->setReadOnly(true);
    m_outputEdit->setVisible(false);
    m_outputEdit->setMinimumHeight(100);
    root->addWidget(m_outputEdit);

    // ── OK button (hidden until push completes) ────────────────────────
    m_okBtn = new QPushButton("OK");
    m_okBtn->setVisible(false);
    connect(m_okBtn, &QPushButton::clicked, this, &QDialog::accept);

    // ── Bottom row ────────────────────────────────────────────────────────
    auto* btnRow = new QHBoxLayout();
    btnRow->addStretch();
    btnRow->addWidget(m_okBtn);

    m_pushBtn = new QPushButton("Push");
    QFont bf = m_pushBtn->font();
    bf.setPointSize(bf.pointSize() + 2);
    bf.setBold(true);
    m_pushBtn->setFont(bf);
    m_pushBtn->setMinimumWidth(100);
    connect(m_pushBtn, &QPushButton::clicked, this, &PushDialog::onPushClicked);
    btnRow->addWidget(m_pushBtn);

    root->addLayout(btnRow);
}

QString PushDialog::findLuaExecutable() const {
    // Try common Lua 5.4+ binary names in PATH order
    static const QStringList candidates = {
        "lua5.4", "lua54", "lua5.3", "lua53", "lua"
    };
    for (const QString& name : candidates) {
        // QProcess::execute with --version returns 0 on success for most lua builds
        QString prog = QStandardPaths::findExecutable(name);
        if (!prog.isEmpty()) return prog;
    }
    return {};
}

// ──────────────────────────────────────────────────────────────────────────────
// Build arguments for push.lua.
//
// Usage:
//   lua push.lua <addonName> <jsonConfigPath> <characterName>-<realm>-<account>
//   lua push.lua <addonName> <jsonConfigPath> all
// ──────────────────────────────────────────────────────────────────────────────
QStringList PushDialog::buildLuaArgs() const {
    QString pushLua    = QCoreApplication::applicationDirPath() + "/push.lua";
    QString jsonConfig = QStandardPaths::writableLocation(QStandardPaths::AppDataLocation) + "/config.json";

    QStringList args;
    args << pushLua << m_addon << jsonConfig;

    if (m_characterDisplay.isEmpty()) {
        args << "all";
    } else {
        // Combine into the single "CharName-Realm-Account" identifier
        args << m_characterDisplay + "-" + m_account;
    }
    return args;
}

void PushDialog::onPushClicked() {
    QString lua = findLuaExecutable();
    if (lua.isEmpty()) {
        QMessageBox::critical(this, "Lua Not Found",
            "Could not find a Lua 5.4 (or higher) interpreter in your PATH.\n"
            "Please install Lua 5.4 and ensure it is accessible.");
        return;
    }

    QStringList args = buildLuaArgs();
    if (args.isEmpty()) {
        QMessageBox::critical(this, "Error", "push.lua not found in the application directory.");
        return;
    }

    m_pushBtn->setEnabled(false);
    m_outputEdit->setVisible(true);
    m_outputEdit->setPlainText("Running push…");
    // Expand the dialog immediately so the output area doesn't overlap the button.
    adjustSize();

    m_process = new QProcess(this);
    connect(m_process,
            QOverload<int, QProcess::ExitStatus>::of(&QProcess::finished),
            this, [this](int code, QProcess::ExitStatus) { onProcessFinished(code); });

    // args[0] = path to push.lua; remaining args are script arguments
    QString script = args.takeFirst();
    m_process->start(lua, QStringList() << script << args);
}

void PushDialog::onProcessFinished(int exitCode) {
    QString stdOut = QString::fromUtf8(m_process->readAllStandardOutput()).trimmed();
    QString stdErr = QString::fromUtf8(m_process->readAllStandardError()).trimmed();

    bool success = (exitCode == 0);

    if (success) {
        QString msg = stdOut.isEmpty() ? "Push completed successfully." : stdOut;
        showResult(true, msg);

        // Log the push
        QString who = m_characterDisplay.isEmpty() ? "All Characters" : m_characterDisplay;
        Logger::instance().log(Logger::PUSH,
            QString("%1 - %2").arg(m_addon, who));
    } else {
        QString errMsg = stdErr.isEmpty() ? stdOut : stdErr;
        if (errMsg.isEmpty()) errMsg = QString("push.lua exited with code %1.").arg(exitCode);
        showResult(false, errMsg);
    }
}

void PushDialog::showResult(bool success, const QString& message) {
    m_outputEdit->setPlainText(message);
    if (!success) {
        m_outputEdit->setStyleSheet("color: #cc2222;");
    }
    m_pushBtn->setVisible(false);
    m_okBtn->setVisible(true);
}

