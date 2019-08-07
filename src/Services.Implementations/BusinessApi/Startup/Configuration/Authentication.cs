using Nexus.Link.Libraries.Core.Platform.Authentication;

#pragma warning disable 1591

namespace Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration
{
    public class Authentication : ConfigurationBase
    {
        public Authentication(ConfigurationBase parent, string sectionName)
            :base(parent, sectionName)
        {
        }
        public string Endpoint => GetMandatoryValue<string>("Endpoint");

        public string ClientId => GetMandatoryValue<string>("ClientId");

        public string ClientSecret => GetMandatoryValue<string>("ClientSecret");

        public AuthenticationCredentials Credentials => new AuthenticationCredentials { ClientId = ClientId, ClientSecret = ClientSecret };
    }
}