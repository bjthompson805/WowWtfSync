using System.Diagnostics;
using System.Linq;

namespace WowWtfSync.WindowsApp
{
    public class AddedCharacter : TableLayoutPanel
    {
        private AddedCharactersPanel parentPanel;
        private string connectedRealm;
        public string characterName;
        public string realm;
        public string account;
        public string faction;

        public AddedCharacter(
            string characterName,
            string realm,
            string account,
            string faction,
            AddedCharactersPanel parentPanel
        )
        {
            this.characterName = characterName;
            this.realm = realm;
            this.account = account;
            this.faction = faction;
            this.parentPanel = parentPanel;

            // Create the table layout panel for this character
            this.RowCount = 1;
            this.ColumnCount = 6;
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 550));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 650));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            this.AutoSize = true;

            // Create the button to remove this character
            Button removeCharacterButton = new Button();
            ToolTip removeCharacterButtonTooltip = new ToolTip();
            removeCharacterButtonTooltip.SetToolTip(
                removeCharacterButton,
                "Remove Character"
            );
            removeCharacterButton.AutoSize = true;
            removeCharacterButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            removeCharacterButton.Anchor = AnchorStyles.Left;
            removeCharacterButton.Image = this.parentPanel.addedCharacterImageList.Images[
                "cancelButtonImage"
            ];
            removeCharacterButton.Click += new EventHandler(this.RemoveCharacterButton_Click);
            this.Controls.Add(removeCharacterButton);

            // Create the button to push this character's data to all other accounts
            Button pushCharacterButton = new Button();
            ToolTip pushCharacterButtonTooltip = new ToolTip();
            pushCharacterButtonTooltip.SetToolTip(
                pushCharacterButton,
                "Open the push configuration dialog for pushing this character's data " +
                "to all other accounts."
            );
            pushCharacterButton.AutoSize = true;
            pushCharacterButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pushCharacterButton.Anchor = AnchorStyles.Left;
            pushCharacterButton.Image = this.parentPanel.addedCharacterImageList.Images[
                "pushButtonImage"
            ];
            pushCharacterButton.Click += new EventHandler(this.PushCharacterButton_Click);
            this.Controls.Add(pushCharacterButton);

            // Create the label for this character
            Label characterToAccountLabel = new Label();
            characterToAccountLabel.AutoSize = true;
            characterToAccountLabel.Text = this.characterName + "-" + this.realm +
                " = " + this.account;
            characterToAccountLabel.Anchor = AnchorStyles.Left;
            this.Controls.Add(characterToAccountLabel);

            // Create the faction radio buttons
            RadioButton allianceRadioButton = new RadioButton();
            allianceRadioButton.AutoSize = true;
            allianceRadioButton.Text = "Alliance";
            allianceRadioButton.Checked = this.faction == "Alliance";
            allianceRadioButton.CheckedChanged += new EventHandler(
                this.FactionRadioButton_CheckedChanged
            );
            RadioButton hordeRadioButton = new RadioButton();
            hordeRadioButton.AutoSize = true;
            hordeRadioButton.Text = "Horde";
            hordeRadioButton.Checked = this.faction == "Horde";
            hordeRadioButton.CheckedChanged += new EventHandler(
                this.FactionRadioButton_CheckedChanged
            );
            TableLayoutPanel factionTableLayoutPanel = new TableLayoutPanel();
            factionTableLayoutPanel.AutoSize = true;
            factionTableLayoutPanel.ColumnCount = 2;
            factionTableLayoutPanel.Dock = DockStyle.Fill;
            factionTableLayoutPanel.Controls.Add(allianceRadioButton);
            factionTableLayoutPanel.Controls.Add(hordeRadioButton);
            factionTableLayoutPanel.Anchor = AnchorStyles.Left;
            this.Controls.Add(factionTableLayoutPanel);

            // Create the "Connected Realm" checkbox
            CheckBox connectedRealmCheckBox = new CheckBox();
            connectedRealmCheckBox.Name = "connectedRealmCheckBox";
            connectedRealmCheckBox.AutoSize = true;
            connectedRealmCheckBox.Text = "Connected Realm";
            connectedRealmCheckBox.CheckedChanged += this.ConnectedRealmCheckBox_CheckedChanged;
            TableLayoutPanel connectedRealmTableLayoutPanel = new TableLayoutPanel();
            connectedRealmTableLayoutPanel.Name = "connectedRealmTableLayoutPanel";
            connectedRealmTableLayoutPanel.AutoSize = true;
            connectedRealmTableLayoutPanel.RowCount = 1;
            connectedRealmTableLayoutPanel.ColumnCount = 2;
            //connectedRealmTableLayoutPanel.Size = new Size(600, 34);
            connectedRealmTableLayoutPanel.Dock = DockStyle.Fill;
            connectedRealmTableLayoutPanel.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
            connectedRealmTableLayoutPanel.Controls.Add(connectedRealmCheckBox);
            connectedRealmTableLayoutPanel.Anchor = AnchorStyles.Left;
            this.Controls.Add(connectedRealmTableLayoutPanel);
        }

        private void ConnectedRealmCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox clickedConnectedRealmCheckBox = (CheckBox)sender;
            List<TableLayoutPanel> tableLayoutPanelsForCharsOfRealm = this.parentPanel.addedCharacters.FindAll(
                addedCharacter => addedCharacter.realm == this.realm
            )
            .Select(
                addedCharacter => (TableLayoutPanel)addedCharacter.Controls
                    .Find("connectedRealmTableLayoutPanel", true)
                    .First()
            ).ToList();
            if (clickedConnectedRealmCheckBox.Checked)
            {
                // Check all of the other connected realm checkboxes with the same realm as this character's realm
                if (this.parentPanel.availableRealms == null)
                {
                    this.parentPanel.availableRealms = this.parentPanel.addedCharacters.Select(
                        addedCharacter => addedCharacter.realm
                    ).Distinct().Order().ToList();
                }

                List<TableLayoutPanel> tableLayoutPanelsForCharsNotOfRealm = this.parentPanel.addedCharacters.FindAll(
                    addedCharacter => addedCharacter.realm != this.realm
                )
                .Select(
                    addedCharacter => (TableLayoutPanel)addedCharacter.Controls
                        .Find("connectedRealmTableLayoutPanel", true)
                        .First()
                ).ToList();

                foreach (TableLayoutPanel tableLayoutPanelForCharNotOfRealm in tableLayoutPanelsForCharsNotOfRealm)
                {
                    ComboBox? comboBoxForCharNotOfRealm = (ComboBox?)tableLayoutPanelForCharNotOfRealm.Controls
                        .Find("connectedRealmComboBox", false)
                        .FirstOrDefault();
                    comboBoxForCharNotOfRealm?.Items.Remove(this.realm);
                }
                this.parentPanel.availableRealms.Remove(this.realm);

                foreach (TableLayoutPanel tableLayoutPanelForCharOfRealm in tableLayoutPanelsForCharsOfRealm)
                {
                    CheckBox checkBoxForCharOfRealm = (CheckBox)tableLayoutPanelForCharOfRealm.Controls
                        .Find("connectedRealmCheckBox", false)
                        .First();
                    checkBoxForCharOfRealm.CheckedChanged -= ((AddedCharacter)tableLayoutPanelForCharOfRealm.Parent).ConnectedRealmCheckBox_CheckedChanged;
                    checkBoxForCharOfRealm.Checked = true;
                    checkBoxForCharOfRealm.CheckedChanged += ((AddedCharacter)tableLayoutPanelForCharOfRealm.Parent).ConnectedRealmCheckBox_CheckedChanged;

                    // Add connected realm selection dropdown
                    ComboBox connectedRealmComboBox = new ComboBox();
                    connectedRealmComboBox.Name = "connectedRealmComboBox";
                    connectedRealmComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                    foreach (string availableRealm in this.parentPanel.availableRealms)
                    {
                        if (availableRealm != this.realm)
                        {
                            connectedRealmComboBox.Items.Add(availableRealm);
                        }
                    }
                    //connectedRealmComboBox.SelectedIndex = 0; // DEBUG
                    connectedRealmComboBox.Width = 300;
                    connectedRealmComboBox.Font = new Font(
                        connectedRealmComboBox.Font.FontFamily,
                        connectedRealmComboBox.Font.Size - 2,
                        connectedRealmComboBox.Font.Style
                    );
                    connectedRealmComboBox.SelectedIndexChanged += ((AddedCharacter)tableLayoutPanelForCharOfRealm.Parent).ConnectedRealmComboBox_SelectedIndexChanged;
                    tableLayoutPanelForCharOfRealm.Controls.Add(connectedRealmComboBox);
                }
            }
            else
            {
                // Checkbox was unchecked

                List<TableLayoutPanel> tableLayoutPanelsForCharsNotOfRealm = this.parentPanel.addedCharacters.FindAll(
                    addedCharacter => addedCharacter.realm != this.realm
                )
                .Select(
                    addedCharacter => (TableLayoutPanel)addedCharacter.Controls
                        .Find("connectedRealmTableLayoutPanel", true)
                        .First()
                ).ToList();
                
                foreach (TableLayoutPanel tableLayoutPanelForCharNotOfRealm in tableLayoutPanelsForCharsNotOfRealm)
                {
                    ComboBox? comboBoxForCharNotOfRealm = (ComboBox?)tableLayoutPanelForCharNotOfRealm.Controls
                        .Find("connectedRealmComboBox", false)
                        .FirstOrDefault();
                    comboBoxForCharNotOfRealm?.Items.Add(this.realm);
                }
                this.parentPanel.availableRealms.Add(this.realm);

                // Uncheck all of the other connected realm checkboxes with the same realm as this character's realm
                for (int i = tableLayoutPanelsForCharsOfRealm.Count - 1; i >= 0; i--)
                {
                    TableLayoutPanel tableLayoutPanelForCharOfRealm = tableLayoutPanelsForCharsOfRealm[i];
                    CheckBox checkBoxForCharOfRealm = (CheckBox)tableLayoutPanelForCharOfRealm.Controls
                        .Find("connectedRealmCheckBox", false)
                        .First();

                    checkBoxForCharOfRealm.CheckedChanged -= ((AddedCharacter)tableLayoutPanelForCharOfRealm.Parent).ConnectedRealmCheckBox_CheckedChanged;
                    checkBoxForCharOfRealm.Checked = false;
                    checkBoxForCharOfRealm.CheckedChanged += ((AddedCharacter)tableLayoutPanelForCharOfRealm.Parent).ConnectedRealmCheckBox_CheckedChanged;

                    // Remove connected realm selection dropdown
                    ComboBox comboBoxForCharOfRealm = (ComboBox)tableLayoutPanelForCharOfRealm.Controls
                        .Find("connectedRealmComboBox", false)
                        .First();
                    string deselectedRealm = (string)comboBoxForCharOfRealm.SelectedItem;
                    if (tableLayoutPanelForCharOfRealm.Controls.Count > 1)
                    {
                        tableLayoutPanelForCharOfRealm.Controls.RemoveAt(1);
                    }

                    if (i == 0)
                    {
                        this.uncheckCheckboxForDeselectedRealm(deselectedRealm);
                    }
                }
            }
        }

        private void uncheckCheckboxForDeselectedRealm(string deselectedRealm)
        {
            List<TableLayoutPanel> tableLayoutPanelsForCharsNotOfRealm = this.parentPanel.addedCharacters.FindAll(
                addedCharacter => addedCharacter.realm != deselectedRealm
            )
            .Select(
                addedCharacter => (TableLayoutPanel)addedCharacter.Controls
                    .Find("connectedRealmTableLayoutPanel", true)
                    .First()
            ).ToList();

            foreach (TableLayoutPanel tableLayoutPanelForCharNotOfRealm in tableLayoutPanelsForCharsNotOfRealm)
            {
                if (((AddedCharacter)tableLayoutPanelForCharNotOfRealm.Parent).realm == deselectedRealm)
                {
                    // This is one of the greyed-out checkboxes for the realm that is being deselected, so skip it
                    continue;
                }

                ComboBox? comboBoxForCharNotOfRealm = (ComboBox?)tableLayoutPanelForCharNotOfRealm.Controls
                    .Find("connectedRealmComboBox", false)
                    .FirstOrDefault();
                if (comboBoxForCharNotOfRealm != null && comboBoxForCharNotOfRealm.SelectedItem == deselectedRealm)
                {
                    return;
                }
            }

            List<TableLayoutPanel> tableLayoutPanelsForDeselectedRealm = this.parentPanel.addedCharacters.FindAll(
                addedCharacter => addedCharacter.realm == deselectedRealm
            )
            .Select(
                addedCharacter => (TableLayoutPanel)addedCharacter.Controls
                    .Find("connectedRealmTableLayoutPanel", true)
                    .First()
            ).ToList();

            foreach (TableLayoutPanel tableLayoutPanelForDeselectedRealm in tableLayoutPanelsForDeselectedRealm)
            {
                if (tableLayoutPanelForDeselectedRealm.Controls.Count > 1)
                {
                    tableLayoutPanelForDeselectedRealm.Controls.RemoveAt(1);
                }

                CheckBox checkBoxForDeselectedRealm = (CheckBox)tableLayoutPanelForDeselectedRealm.Controls
                    .Find("connectedRealmCheckBox", false)
                    .First();
                checkBoxForDeselectedRealm.CheckedChanged -= ((AddedCharacter)tableLayoutPanelForDeselectedRealm.Parent).ConnectedRealmCheckBox_CheckedChanged;
                checkBoxForDeselectedRealm.Checked = false;
                checkBoxForDeselectedRealm.Enabled = true;
                checkBoxForDeselectedRealm.CheckedChanged += ((AddedCharacter)tableLayoutPanelForDeselectedRealm.Parent).ConnectedRealmCheckBox_CheckedChanged;
            }
        }

        private void ConnectedRealmComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox clickedConnectedRealmComboBox = (ComboBox)sender;

            if (this.connectedRealm != null)
            {
                this.uncheckCheckboxForDeselectedRealm(this.connectedRealm);
            }

            List<TableLayoutPanel> tableLayoutPanelsForCharsOfRealm = this.parentPanel.addedCharacters.FindAll(
                addedCharacter => addedCharacter.realm == this.realm
            )
            .Select(
                addedCharacter => (TableLayoutPanel)addedCharacter.Controls
                    .Find("connectedRealmTableLayoutPanel", true)
                    .First()
            ).ToList();

            foreach (TableLayoutPanel tableLayoutPanelForCharOfRealm in tableLayoutPanelsForCharsOfRealm)
            {
                ComboBox comboBoxForCharOfRealm = (ComboBox)tableLayoutPanelForCharOfRealm.Controls
                    .Find("connectedRealmComboBox", false)
                    .First();
                comboBoxForCharOfRealm.SelectedIndexChanged -= ((AddedCharacter)tableLayoutPanelForCharOfRealm.Parent).ConnectedRealmComboBox_SelectedIndexChanged;
                comboBoxForCharOfRealm.SelectedIndex = clickedConnectedRealmComboBox.SelectedIndex;
                comboBoxForCharOfRealm.SelectedIndexChanged += ((AddedCharacter)tableLayoutPanelForCharOfRealm.Parent).ConnectedRealmComboBox_SelectedIndexChanged;
            }

            List<TableLayoutPanel> tableLayoutPanelsForSelectedRealm = this.parentPanel.addedCharacters.FindAll(
                addedCharacter => addedCharacter.realm == (string)clickedConnectedRealmComboBox.SelectedItem
            )
            .Select(
                addedCharacter => (TableLayoutPanel)addedCharacter.Controls
                    .Find("connectedRealmTableLayoutPanel", true)
                    .First()
            ).ToList();

            foreach (TableLayoutPanel tableLayoutPanelForSelectedRealm in tableLayoutPanelsForSelectedRealm)
            {
                ComboBox? comboBoxForSelectedRealm = (ComboBox?)tableLayoutPanelForSelectedRealm.Controls
                    .Find("connectedRealmComboBox", false)
                    .FirstOrDefault();
                if (comboBoxForSelectedRealm == null)
                {
                    comboBoxForSelectedRealm = new ComboBox();
                    comboBoxForSelectedRealm.Name = "connectedRealmComboBox";
                    comboBoxForSelectedRealm.DropDownStyle = ComboBoxStyle.DropDownList;
                    comboBoxForSelectedRealm.Width = 300;
                    comboBoxForSelectedRealm.Font = new Font(
                        comboBoxForSelectedRealm.Font.FontFamily,
                        comboBoxForSelectedRealm.Font.Size - 2,
                        comboBoxForSelectedRealm.Font.Style
                    );
                    tableLayoutPanelForSelectedRealm.Controls.Add(comboBoxForSelectedRealm);
                    comboBoxForSelectedRealm.Items.Add((string)clickedConnectedRealmComboBox.SelectedItem);
                    comboBoxForSelectedRealm.SelectedIndex = 0;
                    comboBoxForSelectedRealm.Enabled = false;
                }

                CheckBox checkBoxForSelectedRealm = (CheckBox)tableLayoutPanelForSelectedRealm.Controls
                    .Find("connectedRealmCheckBox", false)
                    .First();
                checkBoxForSelectedRealm.CheckedChanged -= ((AddedCharacter)tableLayoutPanelForSelectedRealm.Parent).ConnectedRealmCheckBox_CheckedChanged;
                checkBoxForSelectedRealm.Checked = true;
                checkBoxForSelectedRealm.Enabled = false;
                checkBoxForSelectedRealm.CheckedChanged += ((AddedCharacter)tableLayoutPanelForSelectedRealm.Parent).ConnectedRealmCheckBox_CheckedChanged;
            }

            this.connectedRealm = (string)clickedConnectedRealmComboBox.SelectedItem;
        }

        private void FactionRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton factionRadioButton = (RadioButton)sender;
            if (factionRadioButton.Checked)
            {
                this.faction = factionRadioButton.Text;
                this.parentPanel.SaveAddedCharacters();
            }
        }

        private void RemoveCharacterButton_Click(object sender, EventArgs e)
        {
            string message = $"Are you sure you want to remove {this.characterName}-" +
                $"{this.realm}-{this.account} ({this.faction})?";
            string caption = "Remove character";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, caption, buttons);
            if (result == DialogResult.Yes)
            {
                this.parentPanel.RemoveCharacter(this);
                this.parentPanel.SaveAddedCharacters();
            }
        }

        private void PushCharacterButton_Click(object sender, EventArgs e)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem altoholicMenuItem = new ToolStripMenuItem("Altoholic");
            altoholicMenuItem.Click += (s, args) =>
            {
                PushAltoholicConfigurationForm pushAltoholicForm =
                    new PushAltoholicConfigurationForm(
                        this.characterName,
                        this.realm,
                        this.account
                    );
                pushAltoholicForm.Show();
            };
            ToolStripMenuItem auctionatorMenuItem = new ToolStripMenuItem("Auctionator");
            auctionatorMenuItem.Click += (s, args) =>
            {
                PushAuctionatorConfigurationForm pushAuctionatorForm =
                    new PushAuctionatorConfigurationForm(
                        this.characterName,
                        this.realm,
                        this.account
                    );
                pushAuctionatorForm.Show();
            };
            ToolStripMenuItem bagnonMenuItem = new ToolStripMenuItem("Bagnon");
            bagnonMenuItem.Click += (s, args) =>
            {
                PushBagnonConfigurationForm pushBagnonForm =
                    new PushBagnonConfigurationForm(
                        this.characterName,
                        this.realm,
                        this.account
                    );
                pushBagnonForm.Show();
            };
            ToolStripMenuItem novaWorldBuffsMenuItem = new ToolStripMenuItem("NovaWorldBuffs");
            novaWorldBuffsMenuItem.Click += (s, args) =>
            {
                PushNovaWorldBuffsConfigurationForm pushNovaWorldBuffsForm =
                    new PushNovaWorldBuffsConfigurationForm(
                        this.characterName,
                        this.realm,
                        this.account
                    );
                pushNovaWorldBuffsForm.Show();
            };
            ToolStripMenuItem titanGoldMenuItem = new ToolStripMenuItem("TitanGold");
            titanGoldMenuItem.Click += (s, args) =>
            {
                PushTitanGoldConfigurationForm pushTitanGoldForm =
                    new PushTitanGoldConfigurationForm(
                        this.characterName,
                        this.realm,
                        this.account
                    );
                pushTitanGoldForm.Show();
            };
            contextMenu.Items.Add(altoholicMenuItem);
            contextMenu.Items.Add(auctionatorMenuItem);
            contextMenu.Items.Add(bagnonMenuItem);
            contextMenu.Items.Add(novaWorldBuffsMenuItem);
            contextMenu.Items.Add(titanGoldMenuItem);
            contextMenu.Show(Cursor.Position);
        }
    }
}