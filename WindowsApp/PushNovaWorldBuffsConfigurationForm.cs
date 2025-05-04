namespace WowWtfSync.WindowsApp
{
    public class PushNovaWorldBuffsConfigurationForm : PushConfigurationForm
    {
        public PushNovaWorldBuffsConfigurationForm() { }
        public PushNovaWorldBuffsConfigurationForm(
            string characterName,
            string realm,
            string account
        ) : base(characterName, realm, account) { }

        protected override string GetTitle()
        {
            string title = "Push NovaWorldBuffs";
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
                description = "Push NovaWorldBuffs data from each character " +
                    "to all accounts that are not the same as the character's account.";
            }
            else
            {
                description = $"Push NovaWorldBuffs data from {this.characterName}-" +
                    $"{this.realm}-{this.account} to all accounts that are not the " +
                    "same as the character's account.";

            }

            description += " This will allow you to view characters' world buffs " +
                "and raid lockouts that are on other accounts.";

            description += "\n\nMake sure to log out of all characters that are being pushed " +
                "to, otherwise the data will be overwritten when logout occurs.";

            return description;
        }

        protected override string GetAddonName()
        {
            return "NovaWorldBuffs";
        }
    }
}
