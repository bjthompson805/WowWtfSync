using System.Diagnostics;

namespace WowWtfSync.WindowsApp
{
    public class AddedCharacter : TableLayoutPanel
    {
        private AddedCharactersPanel parentPanel;
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
            this.ColumnCount = 4;
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
            FlowLayoutPanel factionFlowLayoutPanel = new FlowLayoutPanel();
            factionFlowLayoutPanel.Size = new Size(300, 34);
            factionFlowLayoutPanel.Controls.Add(allianceRadioButton);
            factionFlowLayoutPanel.Controls.Add(hordeRadioButton);
            factionFlowLayoutPanel.Anchor = AnchorStyles.Left;
            this.Controls.Add(factionFlowLayoutPanel);
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