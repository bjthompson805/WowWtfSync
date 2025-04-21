namespace WowWtfSync.WindowsApp
{
    public class AddedCharactersPanel : TableLayoutPanel
    {
        public List<AddedCharacter> addedCharacters;
        public string wtfAccountDir;
        public ImageList addedCharacterImageList;

        public AddedCharactersPanel()
        {
            this.addedCharacters = new List<AddedCharacter>();
            this.wtfAccountDir = string.Empty;

            // Create the image list for added character panels
            byte[] cancelButtonImageData = Properties.Resources.Cancel;
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(cancelButtonImageData, 0, cancelButtonImageData.Length);
            Image cancelButtonImage = Image.FromStream(memoryStream);
            this.addedCharacterImageList = new ImageList();
            this.addedCharacterImageList.Images.Add(
                "cancelButtonImage",
                cancelButtonImage
            );
            byte[] pushButtonImageData = Properties.Resources.Push;
            memoryStream = new MemoryStream();
            memoryStream.Write(pushButtonImageData, 0, pushButtonImageData.Length);
            Image pushButtonImage = Image.FromStream(memoryStream);
            this.addedCharacterImageList.Images.Add(
                "pushButtonImage",
                pushButtonImage
            );
            this.addedCharacterImageList.ImageSize = new Size(24, 24);
        }

        public void AddCharacter(
            string characterName,
            string realm,
            string account,
            string faction
        )
        {
            if (this.addedCharacters.Count == 0)
            {
                this.Controls.Find("addedCharactersToolbar", true)[0].Show();
            }

            AddedCharacter addedCharacter = new AddedCharacter(
                characterName,
                realm,
                account,
                faction,
                this
            );
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
                addedCharacterDto => new AddedCharacterDto
                {
                    CharacterName = addedCharacterDto.characterName,
                    Realm = addedCharacterDto.realm,
                    Account = addedCharacterDto.account,
                    Faction = addedCharacterDto.faction
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