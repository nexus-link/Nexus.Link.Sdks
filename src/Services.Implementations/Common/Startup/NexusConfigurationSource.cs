using Microsoft.Extensions.Configuration;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.Common.Startup
{
    /// <summary>
    /// Adds the Nexus configuration database as a configuration source
    /// </summary>
    public class NexusConfigurationSource : IConfigurationSource
    {
        private readonly string _authenticationClientId;
        private readonly IConfigurationService _configurationService;

        /// <inheritdoc />
        public NexusConfigurationSource(string authenticationClientId, IConfigurationService configurationService)
        {
            _authenticationClientId = authenticationClientId;
            _configurationService = configurationService;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new NexusConfigurationProvider(_authenticationClientId, _configurationService);
        }
    }
}