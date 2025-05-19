namespace WowWtfSync.WindowsApp
{
    partial class wowWtfSyncForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(wowWtfSyncForm));
            appPanel = new TableLayoutPanel();
            addCharactersPanel = new TableLayoutPanel();
            addCharacterButton = new Button();
            charactersList = new ListBox();
            accountsList = new ListBox();
            wowWtfFolderPanel = new TableLayoutPanel();
            scanButton = new Button();
            wowWtfFolderLabel = new Label();
            wowWtfFolderTextbox = new TextBox();
            label2 = new Label();
            addedCharactersPanel = new AddedCharactersPanel();
            addedCharactersToolbar = new TableLayoutPanel();
            pushAltoholicButton = new Button();
            globalImageList = new ImageList(components);
            pushNovaWorldBuffsButton = new Button();
            pushAuctionatorButton = new Button();
            removeAllButton = new Button();
            pushBagnonButton = new Button();
            pushBagnonButtonTooltip = new ToolTip(components);
            removeAllButtonTooltip = new ToolTip(components);
            pushAuctionatorButtonTooltip = new ToolTip(components);
            pushTitanGoldButton = new Button();
            appPanel.SuspendLayout();
            addCharactersPanel.SuspendLayout();
            wowWtfFolderPanel.SuspendLayout();
            addedCharactersPanel.SuspendLayout();
            addedCharactersToolbar.SuspendLayout();
            SuspendLayout();
            // 
            // appPanel
            // 
            appPanel.ColumnCount = 1;
            appPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            appPanel.Controls.Add(addCharactersPanel, 0, 2);
            appPanel.Controls.Add(wowWtfFolderPanel, 0, 0);
            appPanel.Controls.Add(addedCharactersPanel, 0, 1);
            appPanel.Dock = DockStyle.Fill;
            appPanel.Location = new Point(0, 0);
            appPanel.Name = "appPanel";
            appPanel.RowCount = 3;
            appPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            appPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            appPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 350F));
            appPanel.Size = new Size(1299, 979);
            appPanel.TabIndex = 22;
            // 
            // addCharactersPanel
            // 
            addCharactersPanel.ColumnCount = 2;
            addCharactersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.458744F));
            addCharactersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62.541256F));
            addCharactersPanel.Controls.Add(addCharacterButton, 1, 1);
            addCharactersPanel.Controls.Add(charactersList, 1, 0);
            addCharactersPanel.Controls.Add(accountsList, 0, 0);
            addCharactersPanel.Dock = DockStyle.Fill;
            addCharactersPanel.Location = new Point(3, 632);
            addCharactersPanel.Name = "addCharactersPanel";
            addCharactersPanel.RowCount = 2;
            addCharactersPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            addCharactersPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            addCharactersPanel.Size = new Size(1293, 344);
            addCharactersPanel.TabIndex = 25;
            // 
            // addCharacterButton
            // 
            addCharacterButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            addCharacterButton.Location = new Point(1130, 267);
            addCharacterButton.Name = "addCharacterButton";
            addCharacterButton.Size = new Size(160, 74);
            addCharacterButton.TabIndex = 23;
            addCharacterButton.Text = "Add";
            addCharacterButton.UseVisualStyleBackColor = true;
            addCharacterButton.Click += addCharacterButton_Click;
            // 
            // charactersList
            // 
            charactersList.Dock = DockStyle.Fill;
            charactersList.FormattingEnabled = true;
            charactersList.Location = new Point(487, 3);
            charactersList.Name = "charactersList";
            charactersList.Size = new Size(803, 258);
            charactersList.TabIndex = 21;
            // 
            // accountsList
            // 
            accountsList.Dock = DockStyle.Fill;
            accountsList.FormattingEnabled = true;
            accountsList.Location = new Point(3, 3);
            accountsList.Name = "accountsList";
            accountsList.Size = new Size(478, 258);
            accountsList.TabIndex = 20;
            accountsList.SelectedIndexChanged += accountsList_SelectedIndexChanged;
            // 
            // wowWtfFolderPanel
            // 
            wowWtfFolderPanel.ColumnCount = 3;
            wowWtfFolderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 208F));
            wowWtfFolderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            wowWtfFolderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108F));
            wowWtfFolderPanel.Controls.Add(scanButton, 2, 1);
            wowWtfFolderPanel.Controls.Add(wowWtfFolderLabel, 0, 1);
            wowWtfFolderPanel.Controls.Add(wowWtfFolderTextbox, 1, 1);
            wowWtfFolderPanel.Controls.Add(label2, 0, 0);
            wowWtfFolderPanel.Dock = DockStyle.Fill;
            wowWtfFolderPanel.Location = new Point(3, 3);
            wowWtfFolderPanel.Name = "wowWtfFolderPanel";
            wowWtfFolderPanel.RowCount = 1;
            wowWtfFolderPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));
            wowWtfFolderPanel.RowStyles.Add(new RowStyle());
            wowWtfFolderPanel.Size = new Size(1293, 114);
            wowWtfFolderPanel.TabIndex = 24;
            // 
            // scanButton
            // 
            scanButton.Anchor = AnchorStyles.Right;
            scanButton.Location = new Point(1188, 68);
            scanButton.Name = "scanButton";
            scanButton.Size = new Size(102, 42);
            scanButton.TabIndex = 12;
            scanButton.Text = "Scan";
            scanButton.UseVisualStyleBackColor = true;
            scanButton.Click += scanButton_Click;
            // 
            // wowWtfFolderLabel
            // 
            wowWtfFolderLabel.Anchor = AnchorStyles.Left;
            wowWtfFolderLabel.Location = new Point(3, 72);
            wowWtfFolderLabel.Name = "wowWtfFolderLabel";
            wowWtfFolderLabel.Size = new Size(199, 35);
            wowWtfFolderLabel.TabIndex = 9;
            wowWtfFolderLabel.Text = "WoW WTF Folder";
            // 
            // wowWtfFolderTextbox
            // 
            wowWtfFolderTextbox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            wowWtfFolderTextbox.Location = new Point(211, 70);
            wowWtfFolderTextbox.Name = "wowWtfFolderTextbox";
            wowWtfFolderTextbox.Size = new Size(971, 39);
            wowWtfFolderTextbox.TabIndex = 8;
            // 
            // label2
            // 
            wowWtfFolderPanel.SetColumnSpan(label2, 3);
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(1287, 65);
            label2.TabIndex = 6;
            label2.Text = "Welcome to WoW WTF Sync! This Windows application will allow you to combine data from your WoW WTF folder across accounts.";
            // 
            // addedCharactersPanel
            // 
            addedCharactersPanel.AutoScroll = true;
            addedCharactersPanel.ColumnCount = 1;
            addedCharactersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            addedCharactersPanel.Controls.Add(addedCharactersToolbar, 0, 0);
            addedCharactersPanel.Dock = DockStyle.Fill;
            addedCharactersPanel.Location = new Point(3, 123);
            addedCharactersPanel.Name = "addedCharactersPanel";
            addedCharactersPanel.RowCount = 2;
            addedCharactersPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            addedCharactersPanel.RowStyles.Add(new RowStyle());
            addedCharactersPanel.Size = new Size(1293, 503);
            addedCharactersPanel.TabIndex = 26;
            // 
            // addedCharactersToolbar
            // 
            addedCharactersToolbar.ColumnCount = 6;
            addedCharactersToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225F));
            addedCharactersToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225F));
            addedCharactersToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F));
            addedCharactersToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F));
            addedCharactersToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
            addedCharactersToolbar.ColumnStyles.Add(new ColumnStyle());
            addedCharactersToolbar.Controls.Add(pushTitanGoldButton, 0, 1);
            addedCharactersToolbar.Controls.Add(pushAltoholicButton, 1, 0);
            addedCharactersToolbar.Controls.Add(pushNovaWorldBuffsButton, 4, 0);
            addedCharactersToolbar.Controls.Add(pushAuctionatorButton, 2, 0);
            addedCharactersToolbar.Controls.Add(removeAllButton, 0, 0);
            addedCharactersToolbar.Controls.Add(pushBagnonButton, 3, 0);
            addedCharactersToolbar.Dock = DockStyle.Fill;
            addedCharactersToolbar.Location = new Point(3, 3);
            addedCharactersToolbar.Name = "addedCharactersToolbar";
            addedCharactersToolbar.RowCount = 2;
            addedCharactersToolbar.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            addedCharactersToolbar.RowStyles.Add(new RowStyle());
            addedCharactersToolbar.Size = new Size(1287, 114);
            addedCharactersToolbar.TabIndex = 0;
            addedCharactersToolbar.Visible = false;
            // 
            // pushAltoholicButton
            // 
            pushAltoholicButton.Dock = DockStyle.Fill;
            pushAltoholicButton.ImageKey = "Push.png";
            pushAltoholicButton.ImageList = globalImageList;
            pushAltoholicButton.Location = new Point(228, 3);
            pushAltoholicButton.Name = "pushAltoholicButton";
            pushAltoholicButton.Size = new Size(219, 49);
            pushAltoholicButton.TabIndex = 5;
            pushAltoholicButton.Text = "Push Altoholic";
            pushAltoholicButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            pushBagnonButtonTooltip.SetToolTip(pushAltoholicButton, "Open the push configuration dialog for pushing all characters' data listed below to all other accounts.");
            pushAltoholicButton.UseVisualStyleBackColor = true;
            pushAltoholicButton.Click += pushAltoholicButton_Click;
            // 
            // globalImageList
            // 
            globalImageList.ColorDepth = ColorDepth.Depth32Bit;
            globalImageList.ImageStream = (ImageListStreamer)resources.GetObject("globalImageList.ImageStream");
            globalImageList.TransparentColor = Color.Transparent;
            globalImageList.Images.SetKeyName(0, "Cancel.png");
            globalImageList.Images.SetKeyName(1, "Push.png");
            // 
            // pushNovaWorldBuffsButton
            // 
            pushNovaWorldBuffsButton.Dock = DockStyle.Fill;
            pushNovaWorldBuffsButton.ImageKey = "Push.png";
            pushNovaWorldBuffsButton.ImageList = globalImageList;
            pushNovaWorldBuffsButton.Location = new Point(953, 3);
            pushNovaWorldBuffsButton.Name = "pushNovaWorldBuffsButton";
            pushNovaWorldBuffsButton.Size = new Size(294, 49);
            pushNovaWorldBuffsButton.TabIndex = 4;
            pushNovaWorldBuffsButton.Text = "Push NovaWorldBuffs";
            pushNovaWorldBuffsButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            pushBagnonButtonTooltip.SetToolTip(pushNovaWorldBuffsButton, "Open the push configuration dialog for pushing all characters' data listed below to all other accounts.");
            pushNovaWorldBuffsButton.UseVisualStyleBackColor = true;
            pushNovaWorldBuffsButton.Click += pushNovaWorldBuffsButton_Click;
            // 
            // pushAuctionatorButton
            // 
            pushAuctionatorButton.Dock = DockStyle.Fill;
            pushAuctionatorButton.ImageKey = "Push.png";
            pushAuctionatorButton.ImageList = globalImageList;
            pushAuctionatorButton.Location = new Point(453, 3);
            pushAuctionatorButton.Name = "pushAuctionatorButton";
            pushAuctionatorButton.Size = new Size(244, 49);
            pushAuctionatorButton.TabIndex = 2;
            pushAuctionatorButton.Text = "Push Auctionator";
            pushAuctionatorButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            pushAuctionatorButtonTooltip.SetToolTip(pushAuctionatorButton, "Open the push configuration dialog for pushing all characters' data listed below to all other accounts.");
            pushAuctionatorButton.UseVisualStyleBackColor = true;
            pushAuctionatorButton.Click += pushAllAuctionatorButton_Click;
            // 
            // removeAllButton
            // 
            removeAllButton.Dock = DockStyle.Fill;
            removeAllButton.ImageKey = "Cancel.png";
            removeAllButton.ImageList = globalImageList;
            removeAllButton.Location = new Point(3, 3);
            removeAllButton.Name = "removeAllButton";
            removeAllButton.Size = new Size(219, 49);
            removeAllButton.TabIndex = 0;
            removeAllButton.Text = "Remove All";
            removeAllButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            removeAllButtonTooltip.SetToolTip(removeAllButton, "Remove all characters listed below.");
            removeAllButton.UseVisualStyleBackColor = true;
            removeAllButton.Click += removeAllButton_Click;
            // 
            // pushBagnonButton
            // 
            pushBagnonButton.Dock = DockStyle.Fill;
            pushBagnonButton.ImageKey = "Push.png";
            pushBagnonButton.ImageList = globalImageList;
            pushBagnonButton.Location = new Point(703, 3);
            pushBagnonButton.Name = "pushBagnonButton";
            pushBagnonButton.Size = new Size(244, 49);
            pushBagnonButton.TabIndex = 1;
            pushBagnonButton.Text = "Push Bagnon";
            pushBagnonButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            pushBagnonButtonTooltip.SetToolTip(pushBagnonButton, "Open the push configuration dialog for pushing all characters' data listed below to all other accounts.");
            pushBagnonButton.UseVisualStyleBackColor = true;
            pushBagnonButton.Click += pushAllBagnonButton_Click;
            // 
            // pushTitanGoldButton
            // 
            pushTitanGoldButton.Dock = DockStyle.Fill;
            pushTitanGoldButton.ImageKey = "Push.png";
            pushTitanGoldButton.ImageList = globalImageList;
            pushTitanGoldButton.Location = new Point(3, 58);
            pushTitanGoldButton.Name = "pushTitanGoldButton";
            pushTitanGoldButton.Size = new Size(219, 53);
            pushTitanGoldButton.TabIndex = 6;
            pushTitanGoldButton.Text = "Push TitanGold";
            pushTitanGoldButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            pushBagnonButtonTooltip.SetToolTip(pushTitanGoldButton, "Open the push configuration dialog for pushing all characters' data listed below to all other accounts.");
            pushTitanGoldButton.UseVisualStyleBackColor = true;
            // 
            // wowWtfSyncForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1299, 979);
            Controls.Add(appPanel);
            Name = "wowWtfSyncForm";
            Text = "WoW WTF Sync";
            appPanel.ResumeLayout(false);
            addCharactersPanel.ResumeLayout(false);
            wowWtfFolderPanel.ResumeLayout(false);
            wowWtfFolderPanel.PerformLayout();
            addedCharactersPanel.ResumeLayout(false);
            addedCharactersToolbar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TableLayoutPanel appPanel;
        private TableLayoutPanel addCharactersPanel;
        private Button addCharacterButton;
        private ListBox charactersList;
        private ListBox accountsList;
        private TableLayoutPanel wowWtfFolderPanel;
        private Button scanButton;
        private Label wowWtfFolderLabel;
        private TextBox wowWtfFolderTextbox;
        private Label label2;
        private AddedCharactersPanel addedCharactersPanel;
        private TableLayoutPanel addedCharactersToolbar;
        private Button removeAllButton;
        private Button pushBagnonButton;
        private ImageList globalImageList;
        private ToolTip removeAllButtonTooltip;
        private ToolTip pushBagnonButtonTooltip;
        private Button pushAuctionatorButton;
        private ToolTip pushAuctionatorButtonTooltip;
        private Button pushNovaWorldBuffsButton;
        private Button pushAltoholicButton;
        private Button pushTitanGoldButton;
    }
}
