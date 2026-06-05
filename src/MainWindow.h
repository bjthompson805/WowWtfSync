#pragma once
#include "CharacterEntry.h"

#include <QMainWindow>
#include <QCloseEvent>
#include <QStringList>
#include <QMap>
#include <QJsonArray>

class QLineEdit;
class QListWidget;
class QTableWidget;
class QStackedWidget;
class QPushButton;
class QLabel;
class QStatusBar;
class QComboBox;
class QCheckBox;

class MainWindow : public QMainWindow {
    Q_OBJECT
public:
    explicit MainWindow(QWidget* parent = nullptr);

signals:
    void signedOut();

protected:
    void closeEvent(QCloseEvent* event) override;

private slots:
    void onMyAccount();
    void onSignOut();
    // WTF path bar
    void onBrowse();
    void onApply();

    // Account / character panels
    void onAccountSelected();
    void onCharacterSelected();
    void onAddCharacter();

    // Table
    void onSaveClicked();
    void onRemoveAllClicked();
    void onPushAllAddon(const QString& addon);

    // Column resize constraint
    void onSectionResized(int logicalIndex, int oldSize, int newSize);

private:
    // ── UI builders ───────────────────────────────────────────────────────
    void buildAccountMenu();
    void buildWtfBar(class QVBoxLayout* root);
    void buildPushAllBar(QVBoxLayout* root);
    void buildCharacterTable(QVBoxLayout* root);
    void buildSelectionPanels(QVBoxLayout* root);
    void buildStatusBar();

    // ── Table helpers ─────────────────────────────────────────────────────
    void addTableRow(const CharacterEntry& entry);
    void removeTableRow(int row);
    void loadTableFromConfig();
    void saveTableToConfig();
    CharacterEntry readRowEntry(int row) const;
    void setUnsavedChanges(bool dirty);
    void updateTableVisibility();

    // ── Scanning helpers ──────────────────────────────────────────────────
    void scanWtf();
    void populateAccountList();
    void populateCharacterList(const QString& account);
    void updateAllAccountCombos();
    void updateStatusBar();

    // ── Dirty-state helpers ───────────────────────────────────────────────
    QJsonArray currentTableState() const;
    void checkForChanges();

    // ── Data ──────────────────────────────────────────────────────────────
    QString                    m_wtfPath;
    QStringList                m_accounts;
    QMap<QString, QStringList> m_accountChars;  // account -> ["CharName-Realm", …]
    bool                       m_hasUnsaved  = false;
    bool                       m_resizing    = false;
    QJsonArray                 m_savedState;

    // ── Widgets ───────────────────────────────────────────────────────────
    QLineEdit*   m_wtfEdit       = nullptr;
    QPushButton* m_applyBtn      = nullptr;

    QStackedWidget* m_tableStack  = nullptr;
    QTableWidget*   m_table       = nullptr;
    QPushButton*    m_saveBtn     = nullptr;

    QListWidget* m_accountList   = nullptr;
    QListWidget* m_charList      = nullptr;
    QPushButton* m_addBtn        = nullptr;

    QStatusBar*  m_statusBar     = nullptr;
};
