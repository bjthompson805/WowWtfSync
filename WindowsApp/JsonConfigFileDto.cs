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

        /// <summary>
        /// Stored refresh token for silent re-authentication on next launch.
        /// Null when the user has not logged in or has explicitly signed out.
        /// </summary>
        public string? RefreshToken { get; set; }

        public JsonConfigFileDto()
        {
            this.AddedCharacters = new List<AddedCharacterDto>();
            this.WowWtfFolder = string.Empty;
        }
    }
}
