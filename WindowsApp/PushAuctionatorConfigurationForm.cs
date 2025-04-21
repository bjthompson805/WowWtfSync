namespace WowWtfSync.WindowsApp
{
    public class PushAuctionatorConfigurationForm : PushConfigurationForm
    {
        public PushAuctionatorConfigurationForm() {}
        public PushAuctionatorConfigurationForm(
            string characterName,
            string realm,
            string account
        ) : base(characterName, realm, account) {}

        protected override string GetTitle()
        {
            string title = "Push Auctionator";
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
                description = "Push Auctionator scan data from each account to all other " +
                    "accounts.";
            }
            else
            {
                description = $"Push Auctionator scan data from the {this.account} " +
                    "account to all other accounts.";
            }

            description += " This will copy any scan days for an item that haven't been seen " +
                "by the destination account. This process may take a few minutes.";

            description += "\n\nMake sure to log out of all characters that are being pushed " +
                "to, otherwise the data will be overwritten when logout occurs.";

            return description;
        }

        protected override string GetAddonName()
        {
            return "Auctionator";
        }
    }
}
