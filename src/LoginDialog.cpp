#include "LoginDialog.h"
#include "AuthManager.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QLabel>
#include <QLineEdit>
#include <QPushButton>
#include <QStackedWidget>
#include <QDesktopServices>
#include <QUrl>

LoginDialog::LoginDialog(QWidget* parent)
    : QDialog(parent)
{
    setWindowTitle("WowWtfSync — Sign In");
    setModal(true);
    setFixedWidth(360);
    setWindowFlags(windowFlags() & ~Qt::WindowContextHelpButtonHint);
    buildUi();

    // Try to restore a previous session immediately on open.
    setLoading(true);
    AuthManager::instance().tryRestoreSession([this](bool ok, QString err) {
        if (ok) {
            accept();
            return;
        }
        setLoading(false);
        if (!err.isEmpty()) showError(err);
    });
}

void LoginDialog::buildUi() {
    auto* root = new QVBoxLayout(this);
    root->setSpacing(10);
    root->setContentsMargins(28, 28, 28, 28);

    // ── Title ─────────────────────────────────────────────────────────────
    auto* titleLabel = new QLabel("WoW WTF Sync");
    QFont tf = titleLabel->font();
    tf.setPointSize(tf.pointSize() + 7);
    tf.setBold(true);
    titleLabel->setFont(tf);
    titleLabel->setAlignment(Qt::AlignCenter);
    root->addWidget(titleLabel);

    auto* subLabel = new QLabel("Sign in to your account");
    subLabel->setAlignment(Qt::AlignCenter);
    subLabel->setStyleSheet("color: palette(mid);");
    root->addWidget(subLabel);
    root->addSpacing(6);

    // ── Stacked widget: page 0 = loading, page 1 = form ──────────────────
    m_stack = new QStackedWidget();

    // Page 0 — loading
    auto* loadPage   = new QWidget();
    auto* loadLayout = new QVBoxLayout(loadPage);
    loadLayout->setAlignment(Qt::AlignCenter);
    loadLayout->setContentsMargins(0, 20, 0, 20);
    auto* loadLabel  = new QLabel("Checking your session…");
    loadLabel->setAlignment(Qt::AlignCenter);
    loadLabel->setStyleSheet("color: palette(mid);");
    loadLayout->addWidget(loadLabel);
    m_stack->addWidget(loadPage);  // index 0

    // Page 1 — login form
    auto* formPage   = new QWidget();
    auto* formLayout = new QVBoxLayout(formPage);
    formLayout->setSpacing(6);
    formLayout->setContentsMargins(0, 0, 0, 0);

    formLayout->addWidget(new QLabel("Email"));
    m_emailEdit = new QLineEdit();
    m_emailEdit->setPlaceholderText("you@example.com");
    formLayout->addWidget(m_emailEdit);

    formLayout->addSpacing(2);
    formLayout->addWidget(new QLabel("Password"));
    m_passwordEdit = new QLineEdit();
    m_passwordEdit->setEchoMode(QLineEdit::Password);
    m_passwordEdit->setPlaceholderText("••••••••");
    connect(m_passwordEdit, &QLineEdit::returnPressed, this, &LoginDialog::onLoginClicked);
    formLayout->addWidget(m_passwordEdit);

    // Error label (hidden until needed; supports HTML links)
    m_errorLabel = new QLabel();
    m_errorLabel->setWordWrap(true);
    m_errorLabel->setTextFormat(Qt::RichText);
    m_errorLabel->setOpenExternalLinks(true);
    m_errorLabel->setStyleSheet("color: #cc2222;");
    m_errorLabel->setVisible(false);
    formLayout->addWidget(m_errorLabel);

    formLayout->addSpacing(2);
    m_loginBtn = new QPushButton("Sign In");
    QFont bf = m_loginBtn->font();
    bf.setBold(true);
    m_loginBtn->setFont(bf);
    m_loginBtn->setMinimumHeight(32);
    connect(m_loginBtn, &QPushButton::clicked, this, &LoginDialog::onLoginClicked);
    formLayout->addWidget(m_loginBtn);

    // ── Links row ─────────────────────────────────────────────────────────
    auto* linksRow = new QHBoxLayout();

    auto makeLink = [](const QString& text, const QString& url) {
        auto* btn = new QPushButton(text);
        btn->setFlat(true);
        btn->setCursor(Qt::PointingHandCursor);
        btn->setStyleSheet(
            "QPushButton { color: palette(link); text-decoration: underline; "
            "border: none; padding: 0; background: transparent; }");
        QObject::connect(btn, &QPushButton::clicked, [url]() {
            QDesktopServices::openUrl(QUrl(url));
        });
        return btn;
    };

    linksRow->addWidget(makeLink("Create account",
                                 "https://wowwtfsync.com/register.html"));
    linksRow->addStretch();
    linksRow->addWidget(makeLink("Forgot password?",
                                 "https://wowwtfsync.com/forgot-password.html"));
    formLayout->addLayout(linksRow);

    m_stack->addWidget(formPage);  // index 1
    root->addWidget(m_stack);
}

void LoginDialog::setLoading(bool loading) {
    m_stack->setCurrentIndex(loading ? 0 : 1);
    if (!loading && m_emailEdit)
        m_emailEdit->setFocus();
    adjustSize();
}

void LoginDialog::showError(const QString& msg) {
    m_errorLabel->setText(msg);
    m_errorLabel->setVisible(!msg.isEmpty());
    adjustSize();
}

void LoginDialog::onLoginClicked() {
    const QString email    = m_emailEdit->text().trimmed();
    const QString password = m_passwordEdit->text();

    if (email.isEmpty() || password.isEmpty()) {
        showError("Please enter your email and password.");
        return;
    }

    showError({});
    m_loginBtn->setEnabled(false);
    m_loginBtn->setText("Signing in…");

    AuthManager::instance().login(email, password, [this](bool ok, QString err) {
        m_loginBtn->setEnabled(true);
        m_loginBtn->setText("Sign In");
        if (ok) {
            accept();
        } else {
            showError(err.isEmpty() ? "Login failed. Please try again." : err);
        }
    });
}
