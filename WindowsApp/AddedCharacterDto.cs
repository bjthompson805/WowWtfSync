namespace WowWtfSync.WindowsApp
{
    /*
     * This class is used to map the properties of the AddedCharacter class
     * to a DTO (Data Transfer Object) for the purpose of serialization into JSON.
     */
    public class AddedCharacterDto
    {
        public string CharacterName { get; set; }
        public string Realm { get; set; }
        public string Account { get; set; }
    }
}
