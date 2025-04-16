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
            wowWtfFolderTextbox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            scanButton = new Button();
            accountsList = new ListBox();
            charactersList = new ListBox();
            addCharacterButton = new Button();
            addedCharactersPanel = new AddedCharactersPanel();
            SuspendLayout();
            // 
            // wowWtfFolderTextbox
            // 
            wowWtfFolderTextbox.Location = new Point(212, 147);
            wowWtfFolderTextbox.Name = "wowWtfFolderTextbox";
            wowWtfFolderTextbox.Size = new Size(886, 39);
            wowWtfFolderTextbox.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 150);
            label1.Name = "label1";
            label1.Size = new Size(199, 32);
            label1.TabIndex = 1;
            label1.Text = "WoW WTF Folder";
            // 
            // label2
            // 
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(1216, 100);
            label2.TabIndex = 4;
            label2.Text = resources.GetString("label2.Text");
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 89);
            label3.Name = "label3";
            label3.Size = new Size(951, 32);
            label3.TabIndex = 5;
            label3.Text = "The first iteration of this program will allow for combining Bagnon data across accounts.";
            // 
            // scanButton
            // 
            scanButton.Location = new Point(1104, 147);
            scanButton.Name = "scanButton";
            scanButton.Size = new Size(120, 39);
            scanButton.TabIndex = 6;
            scanButton.Text = "Scan";
            scanButton.UseVisualStyleBackColor = true;
            scanButton.Click += scanButton_Click;
            // 
            // accountsList
            // 
            accountsList.FormattingEnabled = true;
            accountsList.Location = new Point(12, 426);
            accountsList.Name = "accountsList";
            accountsList.Size = new Size(416, 228);
            accountsList.TabIndex = 10;
            accountsList.SelectedIndexChanged += accountsList_SelectedIndexChanged;
            // 
            // charactersList
            // 
            charactersList.FormattingEnabled = true;
            charactersList.Location = new Point(434, 426);
            charactersList.Name = "charactersList";
            charactersList.Size = new Size(794, 228);
            charactersList.TabIndex = 11;
            // 
            // addCharacterButton
            // 
            addCharacterButton.Location = new Point(1040, 660);
            addCharacterButton.Name = "addCharacterButton";
            addCharacterButton.Size = new Size(188, 80);
            addCharacterButton.TabIndex = 12;
            addCharacterButton.Text = "Add";
            addCharacterButton.UseVisualStyleBackColor = true;
            addCharacterButton.Click += addCharacterButton_Click;
            // 
            // addedCharactersPanel
            // 
            addedCharactersPanel.AutoScroll = true;
            addedCharactersPanel.BorderStyle = BorderStyle.FixedSingle;
            addedCharactersPanel.FlowDirection = FlowDirection.TopDown;
            addedCharactersPanel.Location = new Point(12, 192);
            addedCharactersPanel.Name = "addedCharactersPanel";
            addedCharactersPanel.Size = new Size(1216, 228);
            addedCharactersPanel.TabIndex = 13;
            addedCharactersPanel.WrapContents = false;
            // 
            // wowWtfSyncForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1240, 752);
            Controls.Add(addedCharactersPanel);
            Controls.Add(addCharacterButton);
            Controls.Add(charactersList);
            Controls.Add(accountsList);
            Controls.Add(scanButton);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(wowWtfFolderTextbox);
            Name = "wowWtfSyncForm";
            Text = "WoW WTF Sync";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox wowWtfFolderTextbox;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button scanButton;
        private ListBox accountsList;
        private ListBox charactersList;
        private Button addCharacterButton;
        private AddedCharactersPanel addedCharactersPanel;
    }
}
