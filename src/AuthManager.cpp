#include "AuthManager.h"

#include <QNetworkAccessManager>
#include <QNetworkRequest>
#include <QNetworkReply>
#include <QJsonDocument>
#include <QJsonObject>
#include <QSettings>
#include <QUrl>

const QString AuthManager::kBaseUrl = "https://wowwtfsync.com";

// ─────────────────────────────────────────────────────────────────────────────
// Singleton
// ─────────────────────────────────────────────────────────────────────────────

AuthManager& AuthManager::instance() {
    static AuthManager inst;
    return inst;
}

AuthManager::AuthManager(QObject* parent)
    : QObject(parent)
    , m_nam(new QNetworkAccessManager(this))
{}

// ─────────────────────────────────────────────────────────────────────────────
// Token persistence (QSettings)
// ─────────────────────────────────────────────────────────────────────────────

void AuthManager::storeRefreshToken(const QString& token) {
    QSettings s;
    s.setValue("auth/refreshToken", token);
}

QString AuthManager::loadRefreshToken() const {
    QSettings s;
    return s.value("auth/refreshToken").toString();
}

void AuthManager::clearRefreshToken() {
    QSettings s;
    s.remove("auth/refreshToken");
}

// ─────────────────────────────────────────────────────────────────────────────
// Helper — extract server error message or fall back to Qt error string
// ─────────────────────────────────────────────────────────────────────────────

static QString extractError(QNetworkReply* reply, const QByteArray& body) {
    if (!body.isEmpty()) {
        auto doc = QJsonDocument::fromJson(body);
        QString msg = doc.object().value("error").toString();
        if (!msg.isEmpty()) return msg;
    }
    return reply->errorString();
}

// ─────────────────────────────────────────────────────────────────────────────
// tryRestoreSession
// ─────────────────────────────────────────────────────────────────────────────

void AuthManager::tryRestoreSession(std::function<void(bool, QString)> cb) {
    m_refreshToken = loadRefreshToken();
    if (m_refreshToken.isEmpty()) {
        cb(false, {});
        return;
    }
    refreshAccessToken([this, cb](bool ok) {
        if (!ok) {
            cb(false, "Session expired. Please sign in again.");
            return;
        }
        checkSubscription(cb);
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// login
// ─────────────────────────────────────────────────────────────────────────────

void AuthManager::login(const QString& email, const QString& password,
                        std::function<void(bool, QString)> cb)
{
    QNetworkRequest req(QUrl(kBaseUrl + "/api/auth/login"));
    req.setHeader(QNetworkRequest::ContentTypeHeader, "application/json");

    QJsonObject body;
    body["email"]    = email;
    body["password"] = password;

    auto* reply = m_nam->post(req, QJsonDocument(body).toJson(QJsonDocument::Compact));
    connect(reply, &QNetworkReply::finished, this, [this, reply, cb]() {
        QByteArray data = reply->readAll();
        reply->deleteLater();

        if (reply->error() != QNetworkReply::NoError) {
            cb(false, extractError(reply, data));
            return;
        }

        auto obj       = QJsonDocument::fromJson(data).object();
        m_accessToken  = obj["accessToken"].toString();
        m_refreshToken = obj["refreshToken"].toString();
        storeRefreshToken(m_refreshToken);

        auto userObj        = obj["user"].toObject();
        m_userInfo.id       = userObj["id"].toInt();
        m_userInfo.username = userObj["username"].toString();
        m_userInfo.email    = userObj["email"].toString();

        checkSubscription(cb);
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// refreshAccessToken  (internal)
// ─────────────────────────────────────────────────────────────────────────────

void AuthManager::refreshAccessToken(std::function<void(bool)> cb) {
    QNetworkRequest req(QUrl(kBaseUrl + "/api/auth/refresh"));
    req.setHeader(QNetworkRequest::ContentTypeHeader, "application/json");

    QJsonObject body;
    body["refreshToken"] = m_refreshToken;

    auto* reply = m_nam->post(req, QJsonDocument(body).toJson(QJsonDocument::Compact));
    connect(reply, &QNetworkReply::finished, this, [this, reply, cb]() {
        QByteArray data = reply->readAll();
        reply->deleteLater();

        if (reply->error() != QNetworkReply::NoError) {
            clearRefreshToken();
            m_refreshToken.clear();
            cb(false);
            return;
        }

        auto obj       = QJsonDocument::fromJson(data).object();
        m_accessToken  = obj["accessToken"].toString();
        m_refreshToken = obj["refreshToken"].toString();
        storeRefreshToken(m_refreshToken);
        cb(true);
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// checkSubscription  (internal — used after login/restore)
// ─────────────────────────────────────────────────────────────────────────────

void AuthManager::checkSubscription(std::function<void(bool, QString)> cb) {
    fetchMe([this, cb](bool ok, UserInfo info, QString err) {
        // Auth flow is complete — close any pooled connections so idle sockets
        // don't compete with UI events in the main event loop.
        m_nam->clearConnectionCache();

        if (!ok) {
            cb(false, err);
            return;
        }
        m_userInfo     = info;
        m_hasActiveSub = (info.subStatus == "active");
        if (!m_hasActiveSub) {
            cb(false,
               "Your subscription is not active. "
               "Please visit <a href=\"https://wowwtfsync.com/subscribe.html\">"
               "wowwtfsync.com</a> to subscribe.");
            return;
        }
        cb(true, {});
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// fetchMe
// ─────────────────────────────────────────────────────────────────────────────

void AuthManager::fetchMe(std::function<void(bool, UserInfo, QString)> cb, bool retried) {
    QNetworkRequest req(QUrl(kBaseUrl + "/api/auth/me"));
    req.setRawHeader("Authorization", ("Bearer " + m_accessToken).toUtf8());

    auto* reply = m_nam->get(req);
    connect(reply, &QNetworkReply::finished, this, [this, reply, cb, retried]() {
        QByteArray data = reply->readAll();
        reply->deleteLater();

        // On 401, try refreshing the access token once then retry
        int statusCode = reply->attribute(QNetworkRequest::HttpStatusCodeAttribute).toInt();
        if (statusCode == 401 && !retried) {
            refreshAccessToken([this, cb](bool ok) {
                if (!ok) {
                    cb(false, {}, "Session expired. Please sign in again.");
                    return;
                }
                fetchMe(cb, /*retried=*/true);
            });
            return;
        }

        if (reply->error() != QNetworkReply::NoError) {
            cb(false, {}, extractError(reply, data));
            return;
        }

        auto obj = QJsonDocument::fromJson(data).object();
        UserInfo info;
        info.id              = obj["id"].toInt();
        info.username        = obj["username"].toString();
        info.email           = obj["email"].toString();
        info.subStatus       = obj["sub_status"].toString();
        info.licenseKey      = obj["license_key"].toString();
        info.currentPeriodEnd = obj["current_period_end"].toString();
        cb(true, info, {});
    });
}

// ─────────────────────────────────────────────────────────────────────────────
// logout
// ─────────────────────────────────────────────────────────────────────────────

void AuthManager::logout(std::function<void()> cb) {
    auto cleanup = [this, cb]() {
        clearRefreshToken();
        m_accessToken.clear();
        m_refreshToken.clear();
        m_hasActiveSub = false;
        m_userInfo     = {};
        if (cb) cb();
    };

    if (m_accessToken.isEmpty()) {
        cleanup();
        return;
    }

    QNetworkRequest req(QUrl(kBaseUrl + "/api/auth/logout"));
    req.setHeader(QNetworkRequest::ContentTypeHeader, "application/json");
    req.setRawHeader("Authorization", ("Bearer " + m_accessToken).toUtf8());

    QJsonObject body;
    body["refreshToken"] = m_refreshToken;

    auto* reply = m_nam->post(req, QJsonDocument(body).toJson(QJsonDocument::Compact));
    connect(reply, &QNetworkReply::finished, this, [this, reply, cleanup]() {
        reply->deleteLater();
        m_nam->clearConnectionCache();
        cleanup(); // always clean up, even if the server request fails
    });
}
