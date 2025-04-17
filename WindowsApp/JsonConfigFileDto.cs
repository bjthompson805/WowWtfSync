namespace WowWtfSync.WindowsApp
{
    /*
     * This class is used to map the properties of the JsonConfigFile class
     * to a DTO (Data Transfer Object) for the purpose of serialization into JSON.
     */
    public class JsonConfigFileDto
    {
        public List<AddedCharacterDto> AddedCharacters { get; set; }
        public string WowWtfFolder { get; set; }
    }
}
