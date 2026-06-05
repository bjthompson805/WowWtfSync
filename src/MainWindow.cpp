#include "MainWindow.h"
#include "ConfigManager.h"
#include "Logger.h"
#include "WtfScanner.h"
#include "PushDialog.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QGroupBox>
#include <QLabel>
#include <QLineEdit>
#include <QPushButton>
#include <QFileDialog>
#include <QListWidget>
#include <QTableWidget>
#include <QHeaderView>
#include <QComboBox>
#include <QCheckBox>
#include <QMenu>
#include <QAction>
#include <QMessageBox>
#include <QStatusBar>
#include <QFrame>
#include <QSplitter>
#include <QJsonArray>
#include <QJsonObject>
#include <QScrollBar>
#include <QApplication>
#include <QStackedWidget>
#include <QWheelEvent>

// QComboBox subclass that ignores scroll-wheel events so table scrolling
// doesn't accidentally change dropdown values.
class NoScrollComboBox : public QComboBox {
public:
    using QComboBox::QComboBox;
protected:
    void wheelEvent(QWheelEvent* e) override { e->ignore(); }
};


// ─── Column indices ───────────────────────────────────────────────────────────
enum Col {
    COL_ACTIONS   = 0,   // unnamed; contains push (↑) + remove (✕) buttons
    COL_CHARACTER = 1,
    COL_ACCOUNT   = 2,
    COL_FACTION   = 3,
    COL_ENABLED   = 4,
    COL_COUNT     = 5
};

static const QStringList kAddons = {
    "Altoholic", "Auctionator", "Bagnon", "NovaWorldBuffs", "TitanGold"
};

// ─── helpers ─────────────────────────────────────────────────────────────────

/// Wrap a widget in a centered container (used for checkboxes in table cells)
static QWidget* centeredWidget(QWidget* w) {
    auto* container = new QWidget();
    auto* layout    = new QHBoxLayout(container);
    layout->setContentsMargins(0, 0, 0, 0);
    layout->setAlignment(Qt::AlignCenter);
    layout->addWidget(w);
    return container;
}

// ─────────────────────────────────────────────────────────────────────────────
// Construction
// ─────────────────────────────────────────────────────────────────────────────

MainWindow::MainWindow(QWidget* parent) : QMainWindow(parent) {
    setWindowTitle("WowWtfSync");
    setMinimumSize(900, 700);

    auto* central = new QWidget(this);
    setCentralWidget(central);
    auto* root = new QVBoxLayout(central);
    root->setSpacing(8);
    root->setContentsMargins(10, 10, 10, 10);

    buildWtfBar(root);
    buildPushAllBar(root);
    buildCharacterTable(root);
    buildSelectionPanels(root);
    buildStatusBar();

    // Load config
    ConfigManager::instance().load();
    m_wtfPath = ConfigManager::instance().wtfPath();
    m_wtfEdit->setText(m_wtfPath);

    if (!m_wtfPath.isEmpty()) {
        scanWtf();
        populateAccountList();
    }
    loadTableFromConfig();
    updateStatusBar();
    setUnsavedChanges(false);
}

// ─────────────────────────────────────────────────────────────────────────────
// UI builders
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::buildWtfBar(QVBoxLayout* root) {
    auto* row = new QHBoxLayout();
    auto* lbl = new QLabel("WTF Folder:");
    lbl->setFixedWidth(80);
    row->addWidget(lbl);

    m_wtfEdit = new QLineEdit();
    m_wtfEdit->setPlaceholderText("Path to your World of Warcraft/_classic_/WTF folder…");
    row->addWidget(m_wtfEdit, 1);

    auto* browseBtn = new QPushButton("Browse");
    connect(browseBtn, &QPushButton::clicked, this, &MainWindow::onBrowse);
    row->addWidget(browseBtn);

    m_applyBtn = new QPushButton("Apply");
    connect(m_applyBtn, &QPushButton::clicked, this, &MainWindow::onApply);
    row->addWidget(m_applyBtn);

    root->addLayout(row);
}

void MainWindow::buildPushAllBar(QVBoxLayout* root) {
    auto* frame = new QFrame();
    frame->setFrameShape(QFrame::StyledPanel);
    auto* fl = new QVBoxLayout(frame);
    fl->setSpacing(6);
    fl->setContentsMargins(8, 6, 8, 6);

    // "Push All:" + addon buttons (each with an up-arrow icon) on one row
    auto* topRow = new QHBoxLayout();
    auto* pushAllLabel = new QLabel("Push All:");
    QFont f = pushAllLabel->font();
    f.setBold(true);
    pushAllLabel->setFont(f);
    topRow->addWidget(pushAllLabel);
    topRow->addSpacing(8);

    for (const QString& addon : kAddons) {
        auto* btn = new QPushButton("↑ " + addon);
        connect(btn, &QPushButton::clicked, this, [this, addon]() {
            onPushAllAddon(addon);
        });
        topRow->addWidget(btn);
    }
    topRow->addStretch();
    fl->addLayout(topRow);

    root->addWidget(frame);
}

void MainWindow::buildCharacterTable(QVBoxLayout* root) {
    auto* groupBox = new QGroupBox("Added Characters");
    auto* gl = new QVBoxLayout(groupBox);
    gl->setSpacing(4);
    gl->setContentsMargins(6, 6, 6, 6);

    m_table = new QTableWidget(0, COL_COUNT);
    m_table->setHorizontalHeaderLabels({
        "", "Character", "Account", "Faction", "Enabled"
    });

    // ── Header / resize behaviour ─────────────────────────────────────────
    auto* hdr = m_table->horizontalHeader();
    hdr->setSectionResizeMode(QHeaderView::Interactive);
    hdr->setStretchLastSection(false);
    // Prevent the table itself from changing width when columns are dragged
    m_table->setHorizontalScrollBarPolicy(Qt::ScrollBarAlwaysOff);
    m_table->setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Expanding);

    // Column widths
    m_table->setColumnWidth(COL_ACTIONS,    62);
    m_table->setColumnWidth(COL_CHARACTER, 180);
    m_table->setColumnWidth(COL_ACCOUNT,   160);
    m_table->setColumnWidth(COL_FACTION,   100);
    m_table->setColumnWidth(COL_ENABLED,    70);

    // Fixed-size columns (actions + enabled)
    hdr->setSectionResizeMode(COL_ACTIONS, QHeaderView::Fixed);
    hdr->setSectionResizeMode(COL_ENABLED, QHeaderView::Fixed);

    m_table->setSelectionBehavior(QAbstractItemView::SelectRows);
    m_table->setEditTriggers(QAbstractItemView::NoEditTriggers);
    m_table->verticalHeader()->setVisible(false);
    m_table->setSortingEnabled(true);

    connect(hdr, &QHeaderView::sectionResized,
            this, &MainWindow::onSectionResized);

    // ── Empty-state instruction page ─────────────────────────────────────
    auto* emptyPage = new QWidget();
    auto* emptyLayout = new QVBoxLayout(emptyPage);
    emptyLayout->setAlignment(Qt::AlignCenter);
    auto* instrLabel = new QLabel(
        "To get started:\n\n"
        "1. Locate your WTF folder above and click Apply\n"
        "2. Select an account from the Accounts panel below\n"
        "3. Select a character from the Characters panel below\n"
        "4. Click Add");
    instrLabel->setAlignment(Qt::AlignCenter);
    instrLabel->setWordWrap(true);
    QFont instrFont = instrLabel->font();
    instrFont.setPointSize(instrFont.pointSize() + 1);
    instrLabel->setFont(instrFont);
    instrLabel->setStyleSheet("color: palette(mid);");
    emptyLayout->addWidget(instrLabel);

    // ── Stacked widget: page 0 = instructions, page 1 = table ────────────
    m_tableStack = new QStackedWidget();
    m_tableStack->addWidget(emptyPage);  // index 0
    m_tableStack->addWidget(m_table);    // index 1
    m_tableStack->setCurrentIndex(0);
    gl->addWidget(m_tableStack);

    // ── Save / Remove All row ─────────────────────────────────────────────
    auto* savRow = new QHBoxLayout();
    auto* removeAllBtn = new QPushButton("Remove All");
    removeAllBtn->setToolTip("Remove all characters from the list");
    connect(removeAllBtn, &QPushButton::clicked, this, &MainWindow::onRemoveAllClicked);
    savRow->addWidget(removeAllBtn);
    savRow->addStretch();
    m_saveBtn = new QPushButton("Save");
    m_saveBtn->setEnabled(false);
    connect(m_saveBtn, &QPushButton::clicked, this, &MainWindow::onSaveClicked);
    savRow->addWidget(m_saveBtn);
    gl->addLayout(savRow);

    root->addWidget(groupBox, 5);  // stretch factor 5 → table gets most space
}

void MainWindow::buildSelectionPanels(QVBoxLayout* root) {
    auto* frame = new QFrame();
    frame->setFrameShape(QFrame::StyledPanel);
    auto* fl = new QVBoxLayout(frame);
    fl->setContentsMargins(6, 6, 6, 6);
    fl->setSpacing(6);

    // ── Side-by-side panels, equal size ───────────────────────────────────
    auto* panelRow = new QHBoxLayout();
    panelRow->setSpacing(6);

    // Left: accounts
    auto* leftBox = new QGroupBox("Accounts");
    auto* ll = new QVBoxLayout(leftBox);
    m_accountList = new QListWidget();
    m_accountList->setSelectionMode(QAbstractItemView::SingleSelection);
    connect(m_accountList, &QListWidget::currentRowChanged,
            this, &MainWindow::onAccountSelected);
    ll->addWidget(m_accountList);
    panelRow->addWidget(leftBox, 1);   // stretch 1 — equal share

    // Right: characters
    auto* rightBox = new QGroupBox("Characters");
    auto* rl = new QVBoxLayout(rightBox);
    m_charList = new QListWidget();
    m_charList->setSelectionMode(QAbstractItemView::SingleSelection);
    connect(m_charList, &QListWidget::currentRowChanged,
            this, &MainWindow::onCharacterSelected);
    rl->addWidget(m_charList);
    panelRow->addWidget(rightBox, 1);  // stretch 1 — equal share

    fl->addLayout(panelRow);

    // ── Add button — outside both panels, right-justified ─────────────────
    auto* addRow = new QHBoxLayout();
    addRow->addStretch();
    m_addBtn = new QPushButton("Add");
    m_addBtn->setEnabled(false);
    connect(m_addBtn, &QPushButton::clicked, this, &MainWindow::onAddCharacter);
    addRow->addWidget(m_addBtn);
    fl->addLayout(addRow);

    root->addWidget(frame, 3);
}

void MainWindow::buildStatusBar() {
    m_statusBar = statusBar();
    m_statusBar->showMessage("No push history found.");
}

// ─────────────────────────────────────────────────────────────────────────────
// Close event — guard against unsaved changes
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::closeEvent(QCloseEvent* event) {
    if (!m_hasUnsaved) {
        event->accept();
        return;
    }

    auto reply = QMessageBox::question(
        this, "Unsaved Changes",
        "You have unsaved changes to the character list.\n"
        "Are you sure you want to exit without saving?",
        QMessageBox::Yes | QMessageBox::No, QMessageBox::No);

    if (reply == QMessageBox::Yes)
        event->accept();
    else
        event->ignore();
}

// ─────────────────────────────────────────────────────────────────────────────
// WTF scanning
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::scanWtf() {
    m_accounts     = WtfScanner::findAccounts(m_wtfPath);
    m_accountChars = WtfScanner::findAllCharacters(m_wtfPath);
}

void MainWindow::populateAccountList() {
    m_accountList->clear();
    m_accountList->addItems(m_accounts);
    m_charList->clear();
    m_addBtn->setEnabled(false);
}

void MainWindow::populateCharacterList(const QString& account) {
    m_charList->clear();
    m_charList->addItems(m_accountChars.value(account));
    m_addBtn->setEnabled(false);
}

// ─────────────────────────────────────────────────────────────────────────────
// Slots — WTF bar
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::onBrowse() {
    QString dir = QFileDialog::getExistingDirectory(
        this, "Select WTF Folder", m_wtfEdit->text());
    if (!dir.isEmpty()) m_wtfEdit->setText(dir);
}

void MainWindow::onApply() {
    m_wtfPath = m_wtfEdit->text().trimmed();
    ConfigManager::instance().setWtfPath(m_wtfPath);
    ConfigManager::instance().save();

    Logger::instance().log(Logger::CONFIG,
        "WTF path set to: " + m_wtfPath);

    scanWtf();
    populateAccountList();
    updateAllAccountCombos();
}

// ─────────────────────────────────────────────────────────────────────────────
// Slots — selection panels
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::onAccountSelected() {
    auto* item = m_accountList->currentItem();
    if (item) {
        populateCharacterList(item->text());
    }
}

void MainWindow::onCharacterSelected() {
    m_addBtn->setEnabled(m_charList->currentItem() != nullptr);
}

void MainWindow::onAddCharacter() {
    auto* accItem  = m_accountList->currentItem();
    auto* charItem = m_charList->currentItem();
    if (!accItem || !charItem) return;

    QString display = charItem->text();   // "CharName-Realm"
    QString account = accItem->text();

    // Split into name + realm
    int sep = display.lastIndexOf('-');
    CharacterEntry entry;
    entry.account   = account;
    entry.faction   = "Alliance";
    entry.enabled   = true;
    if (sep > 0) {
        entry.characterName = display.left(sep);
        entry.realmName     = display.mid(sep + 1);
    } else {
        entry.characterName = display;
        entry.realmName     = {};
    }

    // Reject duplicates (same character display name + account)
    for (int r = 0; r < m_table->rowCount(); ++r) {
        auto* ci = m_table->item(r, COL_CHARACTER);
        auto* ac = qobject_cast<QComboBox*>(m_table->cellWidget(r, COL_ACCOUNT));
        if (ci && ci->text() == entry.displayName() &&
            ac && ac->currentText() == account) {
            QMessageBox::information(this, "Already Added",
                QString("%1 (%2) has already been added.")
                    .arg(entry.displayName(), account));
            return;
        }
    }

    addTableRow(entry);
    checkForChanges();

    Logger::instance().log(Logger::CHARACTER,
        "Added character: " + entry.displayName() + " (account: " + account + ")");
}

// ─────────────────────────────────────────────────────────────────────────────
// Slots — table actions
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::onSaveClicked() {
    saveTableToConfig();
    // Block selection-list signals while disabling the Save button so that
    // Qt's focus-shift doesn't accidentally fire onAccountSelected and
    // populate the character list as a side-effect.
    m_accountList->blockSignals(true);
    m_charList->blockSignals(true);
    setUnsavedChanges(false);
    m_accountList->blockSignals(false);
    m_charList->blockSignals(false);
    Logger::instance().log(Logger::ACTION, "Character table saved.");
}

void MainWindow::onRemoveAllClicked() {
    if (m_table->rowCount() == 0) return;

    auto reply = QMessageBox::question(
        this, "Remove All Characters",
        "Are you sure you want to remove all characters from the list?",
        QMessageBox::Yes | QMessageBox::No, QMessageBox::No);

    if (reply == QMessageBox::Yes) {
        m_table->setRowCount(0);
        checkForChanges();
        updateTableVisibility();
        Logger::instance().log(Logger::ACTION, "All characters removed.");
    }
}

void MainWindow::onPushAllAddon(const QString& addon) {
    PushDialog dlg(addon, /*all characters*/ "", /*account*/ "",
                   m_wtfPath, m_accounts, this);
    dlg.exec();
    updateStatusBar();
}

// ─────────────────────────────────────────────────────────────────────────────
// Column resize constraint
// Resizing column N steals/gives space to column N+1 (or N-1 for the last col).
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::onSectionResized(int logicalIndex, int oldSize, int newSize) {
    if (m_resizing) return;
    // Only the three middle columns (Character, Account, Faction) are interactive
    if (logicalIndex < COL_CHARACTER || logicalIndex >= COL_ENABLED) return;

    m_resizing = true;

    int diff    = newSize - oldSize;
    int nextCol = logicalIndex + 1;

    if (nextCol < COL_ENABLED) {
        int nextSize = m_table->columnWidth(nextCol) - diff;
        if (nextSize >= 40) {
            m_table->setColumnWidth(nextCol, nextSize);
        } else {
            // Revert — don't allow collapse below minimum
            m_table->setColumnWidth(logicalIndex, oldSize);
        }
    }

    m_resizing = false;
}

// ─────────────────────────────────────────────────────────────────────────────
// Table helpers
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::addTableRow(const CharacterEntry& entry) {
    // Temporarily disable sorting to avoid row-index confusion during population
    bool wasSort = m_table->isSortingEnabled();
    m_table->setSortingEnabled(false);

    int row = m_table->rowCount();
    m_table->insertRow(row);

    // COL_ACTIONS — push (↑) + remove (✕) as small square buttons
    auto* actWidget = new QWidget();
    auto* actLayout = new QHBoxLayout(actWidget);
    actLayout->setContentsMargins(2, 2, 2, 2);
    actLayout->setSpacing(2);

    auto* pushBtn = new QPushButton("↑");
    pushBtn->setFixedSize(26, 26);
    pushBtn->setToolTip("Push addon data for this character");
    connect(pushBtn, &QPushButton::clicked, this, [this, actWidget]() {
        for (int r = 0; r < m_table->rowCount(); ++r) {
            if (m_table->cellWidget(r, COL_ACTIONS) != actWidget) continue;
            auto* ci = m_table->item(r, COL_CHARACTER);
            if (!ci) return;
            QString charDisplay = ci->text();
            auto* acCombo = qobject_cast<QComboBox*>(m_table->cellWidget(r, COL_ACCOUNT));
            QString acct  = acCombo ? acCombo->currentText() : QString();
            QMenu menu;
            for (const QString& addon : kAddons) {
                menu.addAction(addon, this, [this, addon, charDisplay, acct]() {
                    PushDialog dlg(addon, charDisplay, acct, m_wtfPath, m_accounts, this);
                    dlg.exec();
                    updateStatusBar();
                });
            }
            menu.exec(QCursor::pos());
            return;
        }
    });
    actLayout->addWidget(pushBtn);

    auto* removeBtn = new QPushButton("✕");
    removeBtn->setFixedSize(26, 26);
    removeBtn->setToolTip("Remove this character");
    connect(removeBtn, &QPushButton::clicked, this, [this, actWidget]() {
        for (int r = 0; r < m_table->rowCount(); ++r) {
            if (m_table->cellWidget(r, COL_ACTIONS) != actWidget) continue;
            auto* ci = m_table->item(r, COL_CHARACTER);
            QString name = ci ? ci->text() : "this character";
            auto reply = QMessageBox::question(
                this, "Remove Character",
                QString("Are you sure you want to remove %1?").arg(name),
                QMessageBox::Yes | QMessageBox::No, QMessageBox::No);
            if (reply == QMessageBox::Yes)
                removeTableRow(r);
            return;
        }
    });
    actLayout->addWidget(removeBtn);

    m_table->setCellWidget(row, COL_ACTIONS, actWidget);
    auto* actItem = new QTableWidgetItem();
    actItem->setFlags(Qt::NoItemFlags);
    m_table->setItem(row, COL_ACTIONS, actItem);

    // COL_CHARACTER — sortable text item
    auto* charItem = new QTableWidgetItem(entry.displayName());
    charItem->setFlags(charItem->flags() & ~Qt::ItemIsEditable);
    m_table->setItem(row, COL_CHARACTER, charItem);

    // COL_ACCOUNT — combobox
    auto* accCombo = new NoScrollComboBox();
    accCombo->addItems(m_accounts);
    int idx = accCombo->findText(entry.account);
    if (idx >= 0) accCombo->setCurrentIndex(idx);
    connect(accCombo, QOverload<int>::of(&QComboBox::currentIndexChanged),
            this, [this](int) { checkForChanges(); });
    m_table->setCellWidget(row, COL_ACCOUNT, accCombo);
    // Also keep a hidden item for sorting
    auto* accItem = new QTableWidgetItem(entry.account);
    accItem->setFlags(Qt::NoItemFlags);
    m_table->setItem(row, COL_ACCOUNT, accItem);
    m_table->setCellWidget(row, COL_ACCOUNT, accCombo);

    // COL_FACTION — combobox
    auto* facCombo = new NoScrollComboBox();
    facCombo->addItems({"Alliance", "Horde"});
    idx = facCombo->findText(entry.faction);
    if (idx >= 0) facCombo->setCurrentIndex(idx);
    connect(facCombo, QOverload<int>::of(&QComboBox::currentIndexChanged),
            this, [this](int) { checkForChanges(); });
    m_table->setCellWidget(row, COL_FACTION, facCombo);
    auto* facItem = new QTableWidgetItem(entry.faction);
    facItem->setFlags(Qt::NoItemFlags);
    m_table->setItem(row, COL_FACTION, facItem);
    m_table->setCellWidget(row, COL_FACTION, facCombo);

    // COL_ENABLED — centred checkbox
    auto* checkbox = new QCheckBox();
    checkbox->setChecked(entry.enabled);
    connect(checkbox, &QCheckBox::toggled, this, [this](bool) { checkForChanges(); });
    m_table->setCellWidget(row, COL_ENABLED, centeredWidget(checkbox));
    auto* enabledItem = new QTableWidgetItem();
    enabledItem->setFlags(Qt::NoItemFlags);
    m_table->setItem(row, COL_ENABLED, enabledItem);

    m_table->setSortingEnabled(wasSort);
    updateTableVisibility();
    m_table->scrollToItem(m_table->item(row, COL_CHARACTER));
}

void MainWindow::removeTableRow(int row) {
    QString display;
    if (auto* ci = m_table->item(row, COL_CHARACTER))
        display = ci->text();

    m_table->removeRow(row);
    checkForChanges();
    updateTableVisibility();

    Logger::instance().log(Logger::CHARACTER,
        "Removed character: " + display);
}

CharacterEntry MainWindow::readRowEntry(int row) const {
    CharacterEntry e;

    if (auto* ci = m_table->item(row, COL_CHARACTER)) {
        QString display = ci->text();
        int sep = display.lastIndexOf('-');
        if (sep > 0) {
            e.characterName = display.left(sep);
            e.realmName     = display.mid(sep + 1);
        } else {
            e.characterName = display;
        }
    }

    if (auto* w = qobject_cast<QComboBox*>(m_table->cellWidget(row, COL_ACCOUNT)))
        e.account = w->currentText();

    if (auto* w = qobject_cast<QComboBox*>(m_table->cellWidget(row, COL_FACTION)))
        e.faction = w->currentText();

    // The enabled checkbox is wrapped in a container widget
    if (auto* container = m_table->cellWidget(row, COL_ENABLED)) {
        auto* cb = container->findChild<QCheckBox*>();
        if (cb) e.enabled = cb->isChecked();
    }

    return e;
}

void MainWindow::saveTableToConfig() {
    QJsonArray arr;
    for (int r = 0; r < m_table->rowCount(); ++r) {
        CharacterEntry e = readRowEntry(r);
        QJsonObject obj;
        obj["characterName"] = e.characterName;
        obj["realmName"]     = e.realmName;
        obj["account"]       = e.account;
        obj["faction"]       = e.faction;
        obj["enabled"]       = e.enabled;
        arr.append(obj);
    }
    ConfigManager::instance().setCharacters(arr);
    ConfigManager::instance().save();
}

void MainWindow::loadTableFromConfig() {
    m_table->setRowCount(0);
    QJsonArray arr = ConfigManager::instance().characters();
    for (const auto& val : arr) {
        QJsonObject obj = val.toObject();
        CharacterEntry e;
        e.characterName = obj["characterName"].toString();
        e.realmName     = obj["realmName"].toString();
        e.account       = obj["account"].toString();
        e.faction       = obj["faction"].toString("Alliance");
        e.enabled       = obj["enabled"].toBool(true);
        addTableRow(e);
    }
}

void MainWindow::setUnsavedChanges(bool dirty) {
    m_hasUnsaved = dirty;
    m_saveBtn->setEnabled(dirty);
    // Capture the current table as the new clean baseline whenever we mark it saved.
    if (!dirty)
        m_savedState = currentTableState();
}

QJsonArray MainWindow::currentTableState() const {
    QJsonArray arr;
    for (int r = 0; r < m_table->rowCount(); ++r) {
        CharacterEntry e = readRowEntry(r);
        QJsonObject obj;
        obj["characterName"] = e.characterName;
        obj["realmName"]     = e.realmName;
        obj["account"]       = e.account;
        obj["faction"]       = e.faction;
        obj["enabled"]       = e.enabled;
        arr.append(obj);
    }
    return arr;
}

void MainWindow::checkForChanges() {
    // Serialize each entry as a pipe-delimited string, then sort both lists
    // so that table column-sort reordering doesn't falsely dirty the state.
    auto toSortedList = [](const QJsonArray& arr) {
        QStringList list;
        for (const auto& v : arr) {
            QJsonObject o = v.toObject();
            list << QString("%1|%2|%3|%4|%5")
                        .arg(o["characterName"].toString(),
                             o["realmName"].toString(),
                             o["account"].toString(),
                             o["faction"].toString(),
                             o["enabled"].toBool() ? "1" : "0");
        }
        list.sort();
        return list;
    };

    bool dirty = (toSortedList(currentTableState()) != toSortedList(m_savedState));
    m_hasUnsaved = dirty;
    m_saveBtn->setEnabled(dirty);
}

void MainWindow::updateTableVisibility() {
    m_tableStack->setCurrentIndex(m_table->rowCount() > 0 ? 1 : 0);
}

// ─────────────────────────────────────────────────────────────────────────────
// Updates all Account comboboxes in the table when accounts are re-scanned
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::updateAllAccountCombos() {
    for (int r = 0; r < m_table->rowCount(); ++r) {
        if (auto* w = qobject_cast<QComboBox*>(m_table->cellWidget(r, COL_ACCOUNT))) {
            QString current = w->currentText();
            w->blockSignals(true);
            w->clear();
            w->addItems(m_accounts);
            int idx = w->findText(current);
            if (idx >= 0) w->setCurrentIndex(idx);
            w->blockSignals(false);
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Status bar
// ─────────────────────────────────────────────────────────────────────────────

void MainWindow::updateStatusBar() {
    QString last = Logger::instance().lastEntry(Logger::PUSH);
    if (last.isEmpty()) {
        m_statusBar->showMessage("No push history found.");
    } else {
        m_statusBar->showMessage("Last push: " + last);
    }
}
