using System;

namespace WowWtfSync.WindowsApp
{
    public class AddedCharactersPanel : FlowLayoutPanel
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
            AddedCharacter addedCharacter = new AddedCharacter(characterName, realm, account, this);
            this.addedCharacters.Add(addedCharacter);
            this.Controls.Add(addedCharacter);
        }

        public void RemoveCharacter(AddedCharacter addedCharacter)
        {
            this.addedCharacters.Remove(addedCharacter);
            this.Controls.Remove(addedCharacter);
        }

        public void Refresh()
        {
            this.Controls.Clear();
            foreach (AddedCharacter addedCharacter in this.addedCharacters)
            {
                this.Controls.Add(addedCharacter);
            }
        }
    }
}