#pragma once
#include <QDialog>

class QLineEdit;
class QPushButton;
class QLabel;
class QStackedWidget;

class LoginDialog : public QDialog {
    Q_OBJECT
public:
    explicit LoginDialog(QWidget* parent = nullptr);

private slots:
    void onLoginClicked();

private:
    void buildUi();
    void setLoading(bool loading);
    void showError(const QString& msg);   // supports rich text / links

    QStackedWidget* m_stack        = nullptr;  // 0 = loading, 1 = form
    QLineEdit*      m_emailEdit    = nullptr;
    QLineEdit*      m_passwordEdit = nullptr;
    QPushButton*    m_loginBtn     = nullptr;
    QLabel*         m_errorLabel   = nullptr;
};
