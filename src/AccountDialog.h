#pragma once
#include <QDialog>
#include "AuthManager.h"

class QLabel;
class QPushButton;

class AccountDialog : public QDialog {
    Q_OBJECT
public:
    explicit AccountDialog(QWidget* parent = nullptr);

private:
    void buildUi();
    void populate(const AuthManager::UserInfo& info);
    void onRefresh();

    // Labels updated when fresh data arrives
    QLabel*      m_subLabel        = nullptr;
    QLabel*      m_periodEndLabel  = nullptr;
    QLabel*      m_licenseLabel    = nullptr;
    QPushButton* m_copyBtn         = nullptr;
    QLabel*      m_statusMsg       = nullptr;
    QPushButton* m_refreshBtn      = nullptr;
};
