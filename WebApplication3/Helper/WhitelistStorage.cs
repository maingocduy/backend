namespace WebApplication3.Helper
{
    public class WhitelistStorage
    {
        public IDictionary<string, DateTime> Whitelist { get; } = new Dictionary<string, DateTime>();
    }
}
