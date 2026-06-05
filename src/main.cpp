#include "MainWindow.h"
#include "LoginDialog.h"

#include <QApplication>
#include <QIcon>
#include <QObject>

// Returned by QApplication::exit() when the user signs out, so the loop
// can restart and show the login dialog again.
static constexpr int kRestartCode = 1001;

int main(int argc, char* argv[]) {
    QApplication app(argc, argv);

    // Application metadata — used by QStandardPaths and QSettings
    app.setOrganizationName("WowWtfSync");
    app.setOrganizationDomain("wowwtfsync.local");
    app.setApplicationName("WowWtfSync");
    app.setApplicationVersion("1.0.0");

    app.setWindowIcon(QIcon(":/app-icon.png"));

    int exitCode = kRestartCode;
    while (exitCode == kRestartCode) {
        // Show login / session-restore dialog
        {
            LoginDialog login;
            if (login.exec() != QDialog::Accepted)
                return 0; // user closed the login dialog
        }

        // Authenticated and subscription verified — show the main window
        MainWindow w;
        QObject::connect(&w, &MainWindow::signedOut, []() {
            QApplication::exit(kRestartCode);
        });
        w.show();
        exitCode = app.exec();
    }

    return 0;
}
