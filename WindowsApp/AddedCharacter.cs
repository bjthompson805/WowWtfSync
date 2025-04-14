using System;
using System.ComponentModel;
using System.Diagnostics;

namespace WowWtfSync.WindowsApp
{
    public class AddedCharacter : FlowLayoutPanel
    {
        private AddedCharactersPanel parentPanel;
        public string characterName;
        public string realm;
        public string account;

        public AddedCharacter(string characterName, string realm, string account, AddedCharactersPanel parentPanel)
        {
            this.characterName = characterName;
            this.realm = realm;
            this.account = account;
            this.parentPanel = parentPanel;

            // Create the flow-layout panel for this character
            this.FlowDirection = FlowDirection.LeftToRight;
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
            byte[] cancelButtonImageData = Properties.Resources.Cancel;
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(cancelButtonImageData, 0, cancelButtonImageData.Length);
            removeCharacterButton.Image = Image.FromStream(memoryStream);
            removeCharacterButton.Click += new EventHandler(this.RemoveCharacterButton_Click);
            this.Controls.Add(removeCharacterButton);

            // Create the button to push this character's data to all other accounts
            Button pushCharacterButton = new Button();
            ToolTip pushCharacterButtonTooltip = new ToolTip();
            pushCharacterButtonTooltip.SetToolTip(
                pushCharacterButton,
                "Push this character's Bagnon data to all other accounts."
            );
            pushCharacterButton.AutoSize = true;
            pushCharacterButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            byte[] pushButtonImageData = Properties.Resources.Push;
            memoryStream = new MemoryStream();
            memoryStream.Write(pushButtonImageData, 0, pushButtonImageData.Length);
            pushCharacterButton.Image = Image.FromStream(memoryStream);
            pushCharacterButton.Click += new EventHandler(this.PushCharacterButton_Click);
            this.Controls.Add(pushCharacterButton);

            // Create the label for this character
            Label characterToAccountLabel = new Label();
            characterToAccountLabel.AutoSize = true;
            characterToAccountLabel.Text = this.characterName + "-" + this.realm +
                " = " + this.account;
            this.Controls.Add(characterToAccountLabel);
        }

        private void RemoveCharacterButton_Click(object sender, EventArgs e)
        {
            this.parentPanel.RemoveCharacter(this);
        }

        private void PushCharacterButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (Process process = new Process())
                {
                    string workingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

                    List<string> argList = new List<string>();
                    argList.Add('"' + @".\push.lua" + '"');
                    argList.Add('"' + parentPanel.wtfAccountDir + '"');
                    argList.Add(this.characterName);
                    argList.Add(this.realm);
                    argList.Add(this.account);
                    argList.Add("Bagnon");
                    argList.Add('"' + Path.Combine(workingDirectory, "config.json") + '"');

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = workingDirectory + @"\LuaApp\lua-5.4.2\lua54.exe";
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.Arguments = String.Join(' ', argList.ToArray());
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    // Synchronously read the standard error/output of the spawned process.
                    StreamReader stdoutReader = process.StandardOutput;
                    string stdout = stdoutReader.ReadToEnd();
                    StreamReader stderrReader = process.StandardError;
                    string stderr = stderrReader.ReadToEnd();

                    if (stderr != "")
                    {
                        MessageBox.Show(
                            stderr,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                    else
                    {
                        MessageBox.Show(
                            stdout,
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }

                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}