using Microsoft.Extensions.Configuration;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration;

namespace Nexus.Link.Services.Implementations.Adapter.Startup.Configuration
{
    /// <summary>
    /// All the configuration values needed for a business API
    /// </summary>
    public class AdapterConfiguration : ConfigurationBase
    {
        /// <inheritdoc />
        public AdapterConfiguration(IConfiguration configuration)
        :base(configuration)
        {
        }

        /// <summary>
        /// The endpoint for the business API
        /// </summary>
        public string BusinessApiEndpoint => GetMandatoryValue<string>("BusinessApi:Endpoint");

        /// <summary>
        /// The endpoint for the business API
        /// </summary>
        public string BusinessApiClientId => GetMandatoryValue<string>("BusinessApi:ClientId");

        /// <summary>
        /// The endpoint for the business API
        /// </summary>
        public string BusinessApiClientSecret=> GetMandatoryValue<string>("BusinessApi:ClientSecret");

        /// <summary>
        /// Credentials constructed from the properties <see cref="BusinessApiClientId"/> and <see cref="BusinessApiClientSecret"/>
        /// </summary>
        public AuthenticationCredentials BusinessApiCredentials => new AuthenticationCredentials { ClientId = BusinessApiClientId, ClientSecret = BusinessApiClientSecret };
    }
}
