#include "AccountDialog.h"
#include "AuthManager.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QGridLayout>
#include <QLabel>
#include <QPushButton>
#include <QDesktopServices>
#include <QUrl>
#include <QApplication>
#include <QClipboard>
#include <QFrame>
#include <QTimer>

AccountDialog::AccountDialog(QWidget* parent)
    : QDialog(parent)
{
    setWindowTitle("My Account");
    setModal(true);
    setMinimumWidth(420);
    setWindowFlags(windowFlags() & ~Qt::WindowContextHelpButtonHint);
    buildUi();

    // Populate from cached data immediately, then refresh from server
    populate(AuthManager::instance().userInfo());
    onRefresh();
}

void AccountDialog::buildUi() {
    const auto& info = AuthManager::instance().userInfo();

    auto* root = new QVBoxLayout(this);
    root->setSpacing(14);
    root->setContentsMargins(24, 24, 24, 24);

    // ── Title ─────────────────────────────────────────────────────────────
    auto* titleLabel = new QLabel("My Account");
    QFont tf = titleLabel->font();
    tf.setPointSize(tf.pointSize() + 4);
    tf.setBold(true);
    titleLabel->setFont(tf);
    root->addWidget(titleLabel);

    auto* divider = new QFrame();
    divider->setFrameShape(QFrame::HLine);
    divider->setFrameShadow(QFrame::Sunken);
    root->addWidget(divider);

    // ── Details grid ──────────────────────────────────────────────────────
    auto* grid = new QGridLayout();
    grid->setColumnStretch(1, 1);
    grid->setVerticalSpacing(10);
    grid->setHorizontalSpacing(16);

    int row = 0;
    auto addField = [&](const QString& labelText, QWidget* valueWidget) {
        auto* lbl = new QLabel(labelText);
        QFont lf = lbl->font();
        lf.setBold(true);
        lbl->setFont(lf);
        lbl->setAlignment(Qt::AlignTop | Qt::AlignLeft);
        grid->addWidget(lbl, row, 0);
        grid->addWidget(valueWidget, row, 1);
        ++row;
    };

    // Username (static — doesn't change)
    auto* userLbl = new QLabel(info.username);
    addField("Username:", userLbl);

    // Email (static)
    auto* emailLbl = new QLabel(info.email);
    emailLbl->setTextInteractionFlags(Qt::TextSelectableByMouse);
    addField("Email:", emailLbl);

    // Subscription status (updated on refresh)
    m_subLabel = new QLabel();
    addField("Subscription:", m_subLabel);

    // Period end (updated on refresh)
    m_periodEndLabel = new QLabel();
    m_periodEndLabel->setStyleSheet("color: palette(mid);");
    addField("Renews / Expires:", m_periodEndLabel);

    // License key row with copy button (updated on refresh)
    auto* keyRow    = new QHBoxLayout();
    keyRow->setContentsMargins(0, 0, 0, 0);
    keyRow->setSpacing(6);
    m_licenseLabel = new QLabel();
    m_licenseLabel->setTextInteractionFlags(Qt::TextSelectableByMouse);
    m_licenseLabel->setStyleSheet("font-family: monospace;");
    keyRow->addWidget(m_licenseLabel, 1);
    m_copyBtn = new QPushButton("Copy");
    m_copyBtn->setFixedWidth(52);
    m_copyBtn->setVisible(false);
    connect(m_copyBtn, &QPushButton::clicked, this, [this]() {
        QApplication::clipboard()->setText(m_licenseLabel->text());
        m_copyBtn->setText("Copied!");
        QTimer::singleShot(1500, m_copyBtn, [this]() { m_copyBtn->setText("Copy"); });
    });
    keyRow->addWidget(m_copyBtn);
    auto* keyWidget = new QWidget();
    keyWidget->setLayout(keyRow);
    addField("License Key:", keyWidget);

    root->addLayout(grid);

    // ── Status / error message ────────────────────────────────────────────
    m_statusMsg = new QLabel();
    m_statusMsg->setStyleSheet("color: palette(mid);");
    m_statusMsg->setVisible(false);
    root->addWidget(m_statusMsg);

    root->addStretch();

    // ── Dashboard link + bottom buttons ──────────────────────────────────
    auto* dashBtn = new QPushButton("Open My Dashboard on Website");
    dashBtn->setCursor(Qt::PointingHandCursor);
    connect(dashBtn, &QPushButton::clicked, this, []() {
        QDesktopServices::openUrl(QUrl("https://wowwtfsync.com/dashboard.html"));
    });
    root->addWidget(dashBtn);

    auto* btnRow = new QHBoxLayout();
    m_refreshBtn = new QPushButton("Refresh");
    connect(m_refreshBtn, &QPushButton::clicked, this, &AccountDialog::onRefresh);
    btnRow->addWidget(m_refreshBtn);
    btnRow->addStretch();
    auto* closeBtn = new QPushButton("Close");
    connect(closeBtn, &QPushButton::clicked, this, &QDialog::accept);
    btnRow->addWidget(closeBtn);
    root->addLayout(btnRow);
}

void AccountDialog::populate(const AuthManager::UserInfo& info) {
    // Subscription status
    if (info.subStatus == "active") {
        m_subLabel->setText("Active");
        m_subLabel->setStyleSheet("color: #22aa22; font-weight: bold;");
    } else if (info.subStatus.isEmpty()) {
        m_subLabel->setText("No subscription");
        m_subLabel->setStyleSheet("color: palette(mid);");
    } else {
        QString display = info.subStatus;
        display[0] = display[0].toUpper();
        m_subLabel->setText(display);
        m_subLabel->setStyleSheet("color: #cc2222; font-weight: bold;");
    }

    // Period end
    if (info.currentPeriodEnd.isEmpty()) {
        m_periodEndLabel->setText("—");
    } else {
        // Server returns an ISO datetime; show just the date part
        m_periodEndLabel->setText(info.currentPeriodEnd.left(10));
    }

    // License key
    if (info.licenseKey.isEmpty()) {
        m_licenseLabel->setText("—");
        m_copyBtn->setVisible(false);
    } else {
        m_licenseLabel->setText(info.licenseKey);
        m_copyBtn->setVisible(true);
        m_copyBtn->setText("Copy");
    }
}

void AccountDialog::onRefresh() {
    m_refreshBtn->setEnabled(false);
    m_refreshBtn->setText("Refreshing…");
    m_statusMsg->setVisible(false);

    AuthManager::instance().fetchMe([this](bool ok, AuthManager::UserInfo info, QString err) {
        m_refreshBtn->setEnabled(true);
        m_refreshBtn->setText("Refresh");
        if (ok) {
            populate(info);
        } else {
            m_statusMsg->setText("Could not refresh: " + err);
            m_statusMsg->setVisible(true);
        }
    });
}
