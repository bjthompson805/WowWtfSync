using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WowWtfSync.WindowsApp
{
    public partial class wowWtfSyncForm : Form
    {
        private List<string> characterDirs; // Full paths of character dirs in all accounts

        public wowWtfSyncForm()
        {
            InitializeComponent();
            this.InitializeApplication();
            this.characterDirs = new List<string>();
        }

        private void InitializeApplication()
        {
            // Read the JSON config file and load the app with its data
            JsonConfigFile.Load();
            wowWtfFolderTextbox.Text = JsonConfigFile.wowWtfFolder;

            string wtfAccountDir = wowWtfFolderTextbox.Text + "\\Account";
            addedCharactersPanel.wtfAccountDir = wtfAccountDir;

            foreach (AddedCharacterDto addedCharacterDto in JsonConfigFile.addedCharacters)
            {
                string characterName = addedCharacterDto.CharacterName;
                string realm = addedCharacterDto.Realm;
                string account = addedCharacterDto.Account;
                string faction = addedCharacterDto.Faction;
                addedCharactersPanel.AddCharacter(characterName, realm, account, faction);
            }

            // Center the app vertically and horizontally
            this.StartPosition = FormStartPosition.CenterScreen;

            // Remove the horizontal scroll bar from the added characters panel
            addedCharactersPanel.AutoScroll = false;
            addedCharactersPanel.HorizontalScroll.Enabled = false;
            addedCharactersPanel.HorizontalScroll.Visible = false;
            addedCharactersPanel.HorizontalScroll.Maximum = 0;
            addedCharactersPanel.AutoScroll = true;

            // Add taskbar/window icon
            string workingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string iconFile = Path.Combine(workingDirectory, "app-icon.ico");
            this.Icon = new Icon(iconFile);

            // Show last pushed message
            PushConfigurationForm.PushConfigurationFormClosed += (s, e) =>
            {
                this.refreshLastPushedMessage();
            };
            this.refreshLastPushedMessage();
        }

        private void refreshLastPushedMessage()
        {

            string lastPushedMessage = Logger.GetLogMessages("push").LastOrDefault();
            if (string.IsNullOrEmpty(lastPushedMessage))
            {
                lastPushedMessage = "No characters have been pushed yet.";
            }
            else
            {
                // Remove everything before the timestamp in the last pushed message
                Regex timestampRegex = new Regex(@"\d+/\d+/\d+ \d+:\d+:\d+");
                Match timestampMatch = timestampRegex.Match(lastPushedMessage);
                if (timestampMatch.Success)
                {
                    lastPushedMessage = lastPushedMessage
                        .Substring(timestampMatch.Index)
                        .Trim();
                }
                else
                {
                    lastPushedMessage = "No timestamp found in the last pushed message.";
                }
            }
            lastPushedLabel.Text = lastPushedMessage;
        }

        private void addCharacterButton_Click(object sender, EventArgs e)
        {
            string selectedAccount = accountsList.SelectedItem?.ToString();
            string selectedCharacter = charactersList.SelectedItem?.ToString();

            string errorMessage = "";
            if (selectedAccount == null)
                errorMessage = "Please select an account.";
            else if (selectedCharacter == null)
                errorMessage = "Please select a character.";

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(
                    errorMessage,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            string[] selectedCharacterSplit = selectedCharacter.Split('-');
            string selectedCharacterName = selectedCharacterSplit[0];
            string selectedRealm = selectedCharacterSplit[1];
            AddedCharacter? previouslyAddedChar = addedCharactersPanel.addedCharacters.Find(
                (AddedCharacter addedChar) => selectedCharacterName == addedChar.characterName &&
                    selectedRealm == addedChar.realm
            );
            bool charIsAlreadyAdded = previouslyAddedChar != null;
            if (charIsAlreadyAdded)
            {
                bool charIsAddedToSelectedAccount = previouslyAddedChar.account.Equals(selectedAccount);
                if (charIsAddedToSelectedAccount)
                    errorMessage = "The selected character has already been added.";
                else
                    errorMessage = "The selected character has already been added to a " +
                        "different account. Please remove it first.";

                MessageBox.Show(
                    errorMessage,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            addedCharactersPanel.AddCharacter(
                selectedCharacterName,
                selectedRealm,
                selectedAccount,
                "Alliance"
            );
            addedCharactersPanel.SaveAddedCharacters();
        }

        private void scanButton_Click(object sender, EventArgs e)
        {
            string wtfAccountDir = wowWtfFolderTextbox.Text + "\\Account";
            List<string> accountDirs = new List<string>();
            try
            {
                accountDirs = Directory.GetDirectories(wtfAccountDir).ToList();
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show(
                    "The specified WTF folder does not exist.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "You do not have permission to access the specified WTF folder.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Save the new WTF folder location in config.json
            JsonConfigFile.wowWtfFolder = wowWtfFolderTextbox.Text;
            JsonConfigFile.Save();

            // Re-read the WTF folder
            this.characterDirs.Clear();
            foreach (string accountDir in accountDirs)
            {
                string[] realmDirs = Directory.GetDirectories(accountDir);

                // Remove the SavedVariables directory
                Regex savedVariablesRegex = new Regex(@"\\SavedVariables$");
                realmDirs = Array.FindAll(
                    realmDirs,
                    realmDir => !savedVariablesRegex.Match(realmDir).Success
                );

                foreach (string realmDir in realmDirs)
                {
                    string[] characterDirs = Directory.GetDirectories(realmDir);
                    foreach (string characterDir in characterDirs)
                    {
                        this.characterDirs.Add(characterDir);
                    }
                }
            }

            // Update the accounts list and clear the characters list
            Dictionary<string, List<string>> accountToCharactersDict =
                GetAccountToCharactersDict();
            List<string> accountsList = accountToCharactersDict.Keys.ToList();
            this.accountsList.Items.Clear();
            this.charactersList.Items.Clear();
            foreach (string account in accountsList.Distinct())
            {
                this.accountsList.Items.Add(account);
            }

            // Notify the added characters panel of the most recent WTF location
            addedCharactersPanel.wtfAccountDir = wtfAccountDir;
        }

        private void accountsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedAccount = accountsList.SelectedItem.ToString();
            Dictionary<string, List<string>> accountToCharactersDict =
                GetAccountToCharactersDict();
            List<string> charactersList = accountToCharactersDict[selectedAccount];
            this.charactersList.Items.Clear();
            foreach (string character in charactersList)
            {
                this.charactersList.Items.Add(character);
            }
        }

        private Dictionary<string, List<string>> GetAccountToCharactersDict()
        {
            Dictionary<string, List<string>> accountToCharactersDict =
                new Dictionary<string, List<string>>();
            foreach (string characterDir in this.characterDirs)
            {
                Regex characterRegex = new Regex(
                    @"\\WTF\\Account\\([^\\]+)\\([^\\]+)\\([^\\]+)$"
                );
                Match characterMatch = characterRegex.Match(characterDir);
                string account = characterMatch.Groups[1].Value;
                string realm = characterMatch.Groups[2].Value;
                string character = characterMatch.Groups[3].Value;

                if (!accountToCharactersDict.ContainsKey(account))
                {
                    accountToCharactersDict.Add(account, new List<string>());
                }
                List<string> charactersList = accountToCharactersDict[account];
                charactersList.Add(character + "-" + realm);
            }
            return accountToCharactersDict;
        }

        private void removeAllButton_Click(object sender, EventArgs e)
        {
            string message = "Are you sure you want to remove all characters?";
            string caption = "Remove all characters";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, caption, buttons);
            if (result == DialogResult.Yes)
            {
                addedCharactersPanel.RemoveAll();
            }
        }

        private void pushAllBagnonButton_Click(object sender, EventArgs e)
        {
            PushBagnonConfigurationForm pushBagnonConfigurationForm =
                new PushBagnonConfigurationForm();
            pushBagnonConfigurationForm.Show();
        }

        private void pushAllAuctionatorButton_Click(object sender, EventArgs e)
        {
            PushAuctionatorConfigurationForm pushAuctionatorConfigurationForm =
                new PushAuctionatorConfigurationForm();
            pushAuctionatorConfigurationForm.Show();
        }

        private void pushTitanGoldButton_Click(object sender, EventArgs e)
        {
            PushTitanGoldConfigurationForm pushTitanGoldConfigurationForm =
                new PushTitanGoldConfigurationForm();
            pushTitanGoldConfigurationForm.Show();
        }

        private void pushNovaWorldBuffsButton_Click(object sender, EventArgs e)
        {
            PushNovaWorldBuffsConfigurationForm pushNovaWorldBuffsConfigurationForm =
                new PushNovaWorldBuffsConfigurationForm();
            pushNovaWorldBuffsConfigurationForm.Show();
        }

        private void pushAltoholicButton_Click(object sender, EventArgs e)
        {
            PushAltoholicConfigurationForm pushAltoholicConfigurationForm =
                new PushAltoholicConfigurationForm();
            pushAltoholicConfigurationForm.Show();
        }
    }
}
