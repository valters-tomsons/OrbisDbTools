namespace OrbisDbTools.PS4.Models
{
    public record UserAccount
    {
        public UserAccount(string idHash)
        {
            IdHash = idHash;
        }

        public string IdHash { get; set; }
    }
}