namespace WowWtfSync.WindowsApp
{
    /*
     * This class is used to map the properties of the AddedCharacter class
     * to a DTO (Data Transfer Object) for the purpose of serialization into JSON.
     */
    public class AddedCharacterDto
    {
        public string CharacterName;
        public string Realm;
        public string Account;
    }
}
