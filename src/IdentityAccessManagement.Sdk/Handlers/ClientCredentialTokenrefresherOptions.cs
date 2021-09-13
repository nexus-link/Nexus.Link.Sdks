namespace IdentityAccessManagement.Sdk.Handlers
{
    public class ClientCredentialTokenrefresherOptions
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
    }
}