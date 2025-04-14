using System;
using System.Diagnostics;
using System.Text.Json;

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
            this.SaveAddedCharacters();
        }

        public void RemoveCharacter(AddedCharacter addedCharacter)
        {
            this.addedCharacters.Remove(addedCharacter);
            this.Controls.Remove(addedCharacter);
            this.SaveAddedCharacters();
        }

        private void SaveAddedCharacters()
        {
            string workingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string jsonFile = Path.Combine(workingDirectory, "config.json");

            // Map and serialize
            var dtoList = this.addedCharacters.Select(ac => new AddedCharacterDto
            {
                CharacterName = ac.characterName,
                Realm = ac.realm,
                Account = ac.account
            }).ToList();
            string jsonString = JsonSerializer.Serialize(dtoList);
            File.WriteAllText(jsonFile, jsonString);
        }
    }
}