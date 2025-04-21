
using System.Diagnostics;

namespace WowWtfSync.WindowsApp
{
    public abstract class PushConfigurationForm : Form
    {
        // Override these methods in the derived class.
        // This method gets the addon name that is used when calling push.lua.
        protected abstract string GetAddonName();
        // This method gets the title shown at the top of the configuration window.
        protected abstract string GetTitle();
        // This method gets the description shown below the title.
        protected abstract string GetDescription();

        protected bool pushAllCharacters;

        // These are only set if pushAllCharacters is false, and always set if it is true.
        protected string characterName;
        protected string realm;
        protected string account;

        public PushConfigurationForm()
        {
            this.pushAllCharacters = true;
            this.InitializeComponent();
            this.Init();
        }

        public PushConfigurationForm(string characterName, string realm, string account)
        {
            this.pushAllCharacters = false;
            this.characterName = characterName;
            this.realm = realm;
            this.account = account;
            this.InitializeComponent();
            this.Init();
        }

        // This method is for initialization that is separate from the designer stuff.
        private void Init()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.pushConfigTitle.Text = this.GetTitle();
            this.pushConfigDescription.Text = this.GetDescription();
            this.Text = "Push Configuration";
        }

        private void InitializeComponent()
        {
            pushConfigTableLayoutPanel = new TableLayoutPanel();
            pushButton = new Button();
            pushConfigTitle = new Label();
            pushConfigDescription = new Label();
            pushConfigOptionsTableLayoutPanel = new TableLayoutPanel();
            pushConfigTableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // pushConfigTableLayoutPanel
            // 
            pushConfigTableLayoutPanel.ColumnCount = 1;
            pushConfigTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pushConfigTableLayoutPanel.Controls.Add(pushButton, 0, 3);
            pushConfigTableLayoutPanel.Controls.Add(pushConfigTitle, 0, 0);
            pushConfigTableLayoutPanel.Controls.Add(pushConfigDescription, 0, 1);
            pushConfigTableLayoutPanel.Controls.Add(pushConfigOptionsTableLayoutPanel, 0, 2);
            pushConfigTableLayoutPanel.Dock = DockStyle.Fill;
            pushConfigTableLayoutPanel.Location = new Point(0, 0);
            pushConfigTableLayoutPanel.Name = "pushConfigTableLayoutPanel";
            pushConfigTableLayoutPanel.Padding = new Padding(10);
            pushConfigTableLayoutPanel.RowCount = 4;
            pushConfigTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
            pushConfigTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 75F));
            pushConfigTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            pushConfigTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            pushConfigTableLayoutPanel.Size = new Size(874, 529);
            pushConfigTableLayoutPanel.TabIndex = 0;
            // 
            // pushButton
            // 
            pushButton.Anchor = AnchorStyles.Right;
            pushButton.Location = new Point(739, 461);
            pushButton.Name = "pushButton";
            pushButton.Size = new Size(122, 55);
            pushButton.TabIndex = 0;
            pushButton.Text = "Push";
            pushButton.UseVisualStyleBackColor = true;
            pushButton.Click += pushButton_Click;
            // 
            // pushConfigTitle
            // 
            pushConfigTitle.AutoSize = true;
            pushConfigTitle.Dock = DockStyle.Fill;
            pushConfigTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            pushConfigTitle.Location = new Point(13, 10);
            pushConfigTitle.Name = "pushConfigTitle";
            pushConfigTitle.Size = new Size(848, 75);
            pushConfigTitle.TabIndex = 1;
            pushConfigTitle.Text = "Push Addon";
            // 
            // pushConfigDescription
            // 
            pushConfigDescription.AutoSize = true;
            pushConfigDescription.Dock = DockStyle.Fill;
            pushConfigDescription.Location = new Point(13, 85);
            pushConfigDescription.Name = "pushConfigDescription";
            pushConfigDescription.Size = new Size(848, 280);
            pushConfigDescription.TabIndex = 2;
            pushConfigDescription.Text = "Addon push description.";
            // 
            // pushConfigOptionsTableLayoutPanel
            // 
            pushConfigOptionsTableLayoutPanel.ColumnCount = 2;
            pushConfigOptionsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pushConfigOptionsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pushConfigOptionsTableLayoutPanel.Dock = DockStyle.Fill;
            pushConfigOptionsTableLayoutPanel.Location = new Point(13, 368);
            pushConfigOptionsTableLayoutPanel.Name = "pushConfigOptionsTableLayoutPanel";
            pushConfigOptionsTableLayoutPanel.RowCount = 2;
            pushConfigOptionsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pushConfigOptionsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pushConfigOptionsTableLayoutPanel.Size = new Size(848, 87);
            pushConfigOptionsTableLayoutPanel.TabIndex = 3;
            // 
            // PushConfigurationForm
            // 
            ClientSize = new Size(874, 529);
            Controls.Add(pushConfigTableLayoutPanel);
            Name = "PushConfigurationForm";
            pushConfigTableLayoutPanel.ResumeLayout(false);
            pushConfigTableLayoutPanel.PerformLayout();
            ResumeLayout(false);

        }
        protected TableLayoutPanel pushConfigTableLayoutPanel;
        protected Button pushButton;
        protected Label pushConfigTitle;
        protected Label pushConfigDescription;
        protected TableLayoutPanel pushConfigOptionsTableLayoutPanel;

        /*
         * This method will run push.lua for the addon.
         */
        private void pushButton_Click(object sender, EventArgs e)
        {
            string addonName = this.GetAddonName();
            string workingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            List<string> argList = new List<string>();
            argList.Add('"' + @".\push.lua" + '"');
            argList.Add(addonName);
            argList.Add('"' + Path.Combine(workingDirectory, "config.json") + '"');

            if (this.pushAllCharacters)
            {
                argList.Add("all");
            }
            else
            {
                argList.Add(this.characterName + "-" + this.realm + "-" + this.account);
            }

            LuaRunner.Run(argList);
        }
    }
}