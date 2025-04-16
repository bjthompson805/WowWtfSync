using System.Diagnostics;
using System.Text.Json;

namespace WowWtfSync.WindowsApp
{
    public static class JsonConfigFile
    {
        /*
         * This static class represents the config.json file for storing the application's
         * settings and data that was set previously by the user.
         */

        public static string wowWtfFolder = "";
        public static List<AddedCharacterDto> addedCharacters = new List<AddedCharacterDto>();

        /*
         * This method reads the config.json file and sets the appropriate class members,
         * or, if the file doesn't exist, it will create it with default values.
         */
        public static void Load()
        {
            string workingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string jsonFile = Path.Combine(workingDirectory, "config.json");
            if (File.Exists(jsonFile))
            {
                string jsonString = File.ReadAllText(jsonFile);
                JsonConfigFileDto jsonConfigFileDto =
                    JsonSerializer.Deserialize<JsonConfigFileDto>(jsonString);
                if (jsonConfigFileDto != null)
                {
                    JsonConfigFile.wowWtfFolder = jsonConfigFileDto.WowWtfFolder;
                    JsonConfigFile.addedCharacters = jsonConfigFileDto.AddedCharacters;
                }
            }
            else
            {
                JsonConfigFile.wowWtfFolder = @"C:\Program Files (x86)\World of Warcraft\_classic_era_\WTF";
                JsonConfigFile.Save();
            }
        }

        /*
         * This method saves the class members of this class into the config.json file.
         */
        public static void Save()
        {
            string workingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string jsonFile = Path.Combine(workingDirectory, "config.json");
            JsonConfigFileDto jsonConfigFileDto = new JsonConfigFileDto
            {
                AddedCharacters = JsonConfigFile.addedCharacters,
                WowWtfFolder = JsonConfigFile.wowWtfFolder
            };
            string jsonString = JsonSerializer.Serialize(jsonConfigFileDto);
            File.WriteAllText(jsonFile, jsonString);
        }
    }
}
