using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

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
            string workingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string jsonFile = Path.Combine(workingDirectory, "config.json");
            if (File.Exists(jsonFile))
            {
                string jsonString = File.ReadAllText(jsonFile);
                var dtoList = JsonSerializer.Deserialize<List<AddedCharacterDto>>(jsonString);
                if (dtoList != null)
                {
                    foreach (var dto in dtoList)
                    {
                        addedCharactersPanel.AddCharacter(dto.CharacterName, dto.Realm, dto.Account);
                    }
                }
            }
            string wtfAccountDir = wowWtfFolderTextbox.Text + "\\Account";
            addedCharactersPanel.wtfAccountDir = wtfAccountDir;
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

            addedCharactersPanel.AddCharacter(selectedCharacterName, selectedRealm, selectedAccount);
        }

        private void scanButton_Click(object sender, EventArgs e)
        {
            string wtfAccountDir = wowWtfFolderTextbox.Text + "\\Account";
            string[] accountDirs = Directory.GetDirectories(wtfAccountDir);

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

                if (!accountToCharactersDict.ContainsKey(account)) {
                    accountToCharactersDict.Add(account, new List<string>());
                }
                List<string> charactersList = accountToCharactersDict[account];
                charactersList.Add(character + "-" + realm);
            }
            return accountToCharactersDict;
        }
    }
}
