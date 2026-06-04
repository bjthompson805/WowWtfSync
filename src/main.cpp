#include "MainWindow.h"

#include <QApplication>
#include <QIcon>
#include <QStyleFactory>

int main(int argc, char* argv[]) {
    QApplication app(argc, argv);

    // Application metadata used by QStandardPaths / config location
    app.setOrganizationName("WowWtfSync");
    app.setOrganizationDomain("wowwtfsync.local");
    app.setApplicationName("WowWtfSync");
    app.setApplicationVersion("1.0.0");

    app.setWindowIcon(QIcon(":/app-icon.png"));

    MainWindow w;
    w.show();

    return app.exec();
}
