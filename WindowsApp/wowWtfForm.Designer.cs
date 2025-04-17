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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(wowWtfSyncForm));
            appPanel = new TableLayoutPanel();
            addCharactersPanel = new TableLayoutPanel();
            addCharacterButton = new Button();
            charactersList = new ListBox();
            accountsList = new ListBox();
            wowWtfFolderPanel = new TableLayoutPanel();
            scanButton = new Button();
            label3 = new Label();
            wowWtfFolderLabel = new Label();
            wowWtfFolderTextbox = new TextBox();
            label2 = new Label();
            addedCharactersPanel = new AddedCharactersPanel();
            appPanel.SuspendLayout();
            addCharactersPanel.SuspendLayout();
            wowWtfFolderPanel.SuspendLayout();
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
            appPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 165F));
            appPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            appPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 350F));
            appPanel.Size = new Size(1240, 752);
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
            addCharactersPanel.Location = new Point(3, 405);
            addCharactersPanel.Name = "addCharactersPanel";
            addCharactersPanel.RowCount = 2;
            addCharactersPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            addCharactersPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            addCharactersPanel.Size = new Size(1234, 344);
            addCharactersPanel.TabIndex = 25;
            // 
            // addCharacterButton
            // 
            addCharacterButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            addCharacterButton.Location = new Point(1071, 267);
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
            charactersList.Location = new Point(465, 3);
            charactersList.Name = "charactersList";
            charactersList.Size = new Size(766, 258);
            charactersList.TabIndex = 21;
            // 
            // accountsList
            // 
            accountsList.Dock = DockStyle.Fill;
            accountsList.FormattingEnabled = true;
            accountsList.Location = new Point(3, 3);
            accountsList.Name = "accountsList";
            accountsList.Size = new Size(456, 258);
            accountsList.TabIndex = 20;
            accountsList.SelectedIndexChanged += accountsList_SelectedIndexChanged;
            // 
            // wowWtfFolderPanel
            // 
            wowWtfFolderPanel.ColumnCount = 3;
            wowWtfFolderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 208F));
            wowWtfFolderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            wowWtfFolderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108F));
            wowWtfFolderPanel.Controls.Add(scanButton, 2, 2);
            wowWtfFolderPanel.Controls.Add(label3, 0, 1);
            wowWtfFolderPanel.Controls.Add(wowWtfFolderLabel, 0, 2);
            wowWtfFolderPanel.Controls.Add(wowWtfFolderTextbox, 1, 2);
            wowWtfFolderPanel.Controls.Add(label2, 0, 0);
            wowWtfFolderPanel.Dock = DockStyle.Fill;
            wowWtfFolderPanel.Location = new Point(3, 3);
            wowWtfFolderPanel.Name = "wowWtfFolderPanel";
            wowWtfFolderPanel.RowCount = 3;
            wowWtfFolderPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            wowWtfFolderPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            wowWtfFolderPanel.RowStyles.Add(new RowStyle());
            wowWtfFolderPanel.Size = new Size(1234, 159);
            wowWtfFolderPanel.TabIndex = 24;
            // 
            // scanButton
            // 
            scanButton.Anchor = AnchorStyles.Right;
            scanButton.Location = new Point(1129, 112);
            scanButton.Name = "scanButton";
            scanButton.Size = new Size(102, 42);
            scanButton.TabIndex = 12;
            scanButton.Text = "Scan";
            scanButton.UseVisualStyleBackColor = true;
            scanButton.Click += scanButton_Click;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            label3.AutoSize = true;
            wowWtfFolderPanel.SetColumnSpan(label3, 3);
            label3.Location = new Point(3, 72);
            label3.Name = "label3";
            label3.Size = new Size(1228, 32);
            label3.TabIndex = 11;
            label3.Text = "The first iteration of this program will allow for combining Bagnon data across accounts.";
            // 
            // wowWtfFolderLabel
            // 
            wowWtfFolderLabel.Anchor = AnchorStyles.Left;
            wowWtfFolderLabel.Location = new Point(3, 116);
            wowWtfFolderLabel.Name = "wowWtfFolderLabel";
            wowWtfFolderLabel.Size = new Size(199, 35);
            wowWtfFolderLabel.TabIndex = 9;
            wowWtfFolderLabel.Text = "WoW WTF Folder";
            // 
            // wowWtfFolderTextbox
            // 
            wowWtfFolderTextbox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            wowWtfFolderTextbox.Location = new Point(211, 114);
            wowWtfFolderTextbox.Name = "wowWtfFolderTextbox";
            wowWtfFolderTextbox.Size = new Size(912, 39);
            wowWtfFolderTextbox.TabIndex = 8;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            wowWtfFolderPanel.SetColumnSpan(label2, 3);
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(1228, 68);
            label2.TabIndex = 6;
            label2.Text = resources.GetString("label2.Text");
            // 
            // addedCharactersPanel
            // 
            addedCharactersPanel.AutoScroll = true;
            addedCharactersPanel.AutoSize = true;
            addedCharactersPanel.BorderStyle = BorderStyle.FixedSingle;
            addedCharactersPanel.Dock = DockStyle.Fill;
            addedCharactersPanel.FlowDirection = FlowDirection.TopDown;
            addedCharactersPanel.Location = new Point(3, 168);
            addedCharactersPanel.Name = "addedCharactersPanel";
            addedCharactersPanel.Size = new Size(1234, 231);
            addedCharactersPanel.TabIndex = 22;
            addedCharactersPanel.WrapContents = false;
            // 
            // wowWtfSyncForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1240, 752);
            Controls.Add(appPanel);
            Name = "wowWtfSyncForm";
            Text = "WoW WTF Sync";
            appPanel.ResumeLayout(false);
            appPanel.PerformLayout();
            addCharactersPanel.ResumeLayout(false);
            wowWtfFolderPanel.ResumeLayout(false);
            wowWtfFolderPanel.PerformLayout();
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
        private Label label3;
        private Label wowWtfFolderLabel;
        private TextBox wowWtfFolderTextbox;
        private Label label2;
        private AddedCharactersPanel addedCharactersPanel;
    }
}
