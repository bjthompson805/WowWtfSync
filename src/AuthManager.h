#pragma once
#include <QObject>
#include <QString>
#include <functional>

class QNetworkAccessManager;

class AuthManager : public QObject {
    Q_OBJECT
public:
    struct UserInfo {
        int     id              = 0;
        QString username;
        QString email;
        QString subStatus;          // "active", "cancelled", or empty
        QString licenseKey;
        QString currentPeriodEnd;   // ISO datetime string from server
    };

    static AuthManager& instance();

    // Attempt to restore a previous session from the stored refresh token.
    // Calls back with (true, "") on success (active subscription), or
    // (false, errorMessage) on failure.
    void tryRestoreSession(std::function<void(bool, QString)> cb);

    // Authenticate with email + password.
    void login(const QString& email, const QString& password,
               std::function<void(bool, QString)> cb);

    // Fetch fresh user info from GET /api/auth/me.
    // Automatically refreshes the access token once on 401.
    void fetchMe(std::function<void(bool, UserInfo, QString)> cb,
                 bool retried = false);

    // Sign out: revokes the refresh token on the server and clears local state.
    void logout(std::function<void()> cb = nullptr);

    bool          isLoggedIn()            const { return !m_accessToken.isEmpty(); }
    bool          hasActiveSubscription() const { return m_hasActiveSub; }
    const UserInfo& userInfo()            const { return m_userInfo; }
    QString       accessToken()           const { return m_accessToken; }

signals:
    void sessionExpired();

private:
    explicit AuthManager(QObject* parent = nullptr);

    // Refresh the access token using the stored refresh token.
    void refreshAccessToken(std::function<void(bool)> cb);

    // After getting tokens, call /me and verify the subscription is active.
    void checkSubscription(std::function<void(bool, QString)> cb);

    // Persist / load / clear the refresh token via QSettings.
    void    storeRefreshToken(const QString& token);
    QString loadRefreshToken() const;
    void    clearRefreshToken();

    QNetworkAccessManager* m_nam          = nullptr;
    QString                m_accessToken;
    QString                m_refreshToken;
    UserInfo               m_userInfo;
    bool                   m_hasActiveSub = false;

    static const QString kBaseUrl;
};
