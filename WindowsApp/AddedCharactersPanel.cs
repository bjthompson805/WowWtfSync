namespace WowWtfSync.WindowsApp
{
    public class AddedCharactersPanel : TableLayoutPanel
    {
        public List<AddedCharacter> addedCharacters;
        public string wtfAccountDir;

        public AddedCharactersPanel()
        {
            this.addedCharacters = new List<AddedCharacter>();
            this.wtfAccountDir = string.Empty;
        }

        public void AddCharacter(string characterName, string realm, string account)
        {
            if (this.addedCharacters.Count == 0)
            {
                this.Controls.Find("addedCharactersToolbar", true)[0].Show();
            }

            AddedCharacter addedCharacter = new AddedCharacter(characterName, realm, account, this);
            this.addedCharacters.Add(addedCharacter);
            this.Controls.Add(addedCharacter);
        }

        public void RemoveCharacter(AddedCharacter addedCharacter)
        {
            this.addedCharacters.Remove(addedCharacter);
            this.Controls.Remove(addedCharacter);

            if (this.addedCharacters.Count == 0)
            {
                this.Controls.Find("addedCharactersToolbar", true)[0].Hide();
            }
        }

        public void SaveAddedCharacters()
        {
            // Map and serialize
            List<AddedCharacterDto> addedCharactersDtoList = this.addedCharacters.Select(
                ac => new AddedCharacterDto
                {
                    CharacterName = ac.characterName,
                    Realm = ac.realm,
                    Account = ac.account
                }
            ).ToList();
            JsonConfigFile.addedCharacters = addedCharactersDtoList;
            JsonConfigFile.Save();
        }

        public void RemoveAll()
        {
            foreach (AddedCharacter addedCharacter in this.addedCharacters)
            {
                this.Controls.Remove(addedCharacter);
            }
            this.addedCharacters.Clear();
            this.Controls.Find("addedCharactersToolbar", true)[0].Hide();
        }
    }
}