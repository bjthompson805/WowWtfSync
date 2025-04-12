using System.Text.RegularExpressions;

namespace WowWtfSync
{
    public partial class wowWtfSyncForm : Form
    {
        private List<string> characterDirs;

        public wowWtfSyncForm()
        {
            InitializeComponent();
            this.characterDirs = new List<string>();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

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
            this.RefreshAccounts();
        }

        private void RefreshAccounts()
        {
            Dictionary<string, List<string>> accountToCharactersDict =
                GetAccountToCharactersDict();
            List<string> accountsList = accountToCharactersDict.Keys.ToList();
            this.accountsList.Items.Clear();
            this.charactersList.Items.Clear();
            foreach (string account in accountsList.Distinct())
            {
                this.accountsList.Items.Add(account);
            }
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
