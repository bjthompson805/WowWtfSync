namespace WowWtfSync
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
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            accountsList = new ListBox();
            charactersList = new ListBox();
            addCharacterButton = new Button();
            charactersToAccountPanel = new Panel();
            SuspendLayout();
            // 
            // wowWtfFolderTextbox
            // 
            wowWtfFolderTextbox.Location = new Point(212, 147);
            wowWtfFolderTextbox.Name = "wowWtfFolderTextbox";
            wowWtfFolderTextbox.Size = new Size(886, 39);
            wowWtfFolderTextbox.TabIndex = 0;
            wowWtfFolderTextbox.Text = "C:\\Program Files (x86)\\World of Warcraft\\_classic_era_\\WTF";
            wowWtfFolderTextbox.TextChanged += textBox1_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 150);
            label1.Name = "label1";
            label1.Size = new Size(199, 32);
            label1.TabIndex = 1;
            label1.Text = "WoW WTF Folder";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(1216, 100);
            label2.TabIndex = 4;
            label2.Text = resources.GetString("label2.Text");
            label2.Click += label2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 89);
            label3.Name = "label3";
            label3.Size = new Size(951, 32);
            label3.TabIndex = 5;
            label3.Text = "The first iteration of this program will allow for combining Bagnon data across accounts.";
            label3.Click += label3_Click;
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
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 276);
            label4.Name = "label4";
            label4.Size = new Size(297, 32);
            label4.TabIndex = 7;
            label4.Text = "Leibnitz-Whitemane -> #2";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 212);
            label5.Name = "label5";
            label5.Size = new Size(356, 32);
            label5.TabIndex = 8;
            label5.Text = "Yellowpweest-Whitemane -> #1";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 244);
            label6.Name = "label6";
            label6.Size = new Size(303, 32);
            label6.TabIndex = 9;
            label6.Text = "Yellowqt-Whitemane -> #1";
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
            addCharacterButton.Click += button2_Click;
            // 
            // charactersToAccountPanel
            // 
            charactersToAccountPanel.Location = new Point(12, 320);
            charactersToAccountPanel.Name = "charactersToAccountPanel";
            charactersToAccountPanel.Size = new Size(1216, 94);
            charactersToAccountPanel.TabIndex = 13;
            // 
            // wowWtfSyncForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1240, 752);
            Controls.Add(charactersToAccountPanel);
            Controls.Add(addCharacterButton);
            Controls.Add(charactersList);
            Controls.Add(accountsList);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(scanButton);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(wowWtfFolderTextbox);
            Name = "wowWtfSyncForm";
            Text = "WoW WTF Sync";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox wowWtfFolderTextbox;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button scanButton;
        private Label label4;
        private Label label5;
        private Label label6;
        private ListBox accountsList;
        private ListBox charactersList;
        private Button addCharacterButton;
        private Panel charactersToAccountPanel;
    }
}
