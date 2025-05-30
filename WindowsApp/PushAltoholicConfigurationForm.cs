namespace WowWtfSync.WindowsApp
{
    public class PushAltoholicConfigurationForm : PushConfigurationForm
    {
        public PushAltoholicConfigurationForm() { }
        public PushAltoholicConfigurationForm(
            string characterName,
            string realm,
            string account
        ) : base(characterName, realm, account) { }

        protected override string GetTitle()
        {
            string title = "Push Altoholic";
            if (this.pushAllCharacters)
            {
                title += " - All Characters";
            }
            else
            {
                title += $" - {this.characterName}";
            }

            return title;
        }

        protected override string GetDescription()
        {
            string description = "";
            if (this.pushAllCharacters)
            {
                description = "Push Altoholic data from all accounts, combining the data.";
            }
            else
            {
                description = $"Push Altoholic data for {this.characterName}-" +
                    $"{this.realm}-{this.account} by combining the data to/from all accounts.";
            }

            description += "\n\nMake sure to log out of all characters that are being pushed " +
                "to, otherwise the data will be overwritten when logout occurs.";

            description += "\n\n*Aggregation of mail sent to your own character from another account " +
                "is also supported.";

            return description;
        }

        protected override string GetAddonName()
        {
            return "Altoholic";
        }
    }
}
