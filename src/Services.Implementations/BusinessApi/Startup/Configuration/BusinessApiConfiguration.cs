using Microsoft.Extensions.Configuration;

namespace Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration
{
    /// <summary>
    /// All the configuration values needed for a business API
    /// </summary>
    public class BusinessApiConfiguration : ConfigurationBase
    {
        /// <inheritdoc />
        public BusinessApiConfiguration(IConfiguration configuration)
        :base(configuration)
        {
            AuthenticationLocal = new Authentication(this, "Authentication:Local");
            AuthenticationNexus = new Authentication(this, "Authentication:Nexus");
            NexusCapabilityEndpoints = new NexusCapabilityEndpoints(this, "NexusCapabilityEndpoints");
        }

        /// <summary>
        /// Configuration for local authentication
        /// </summary>
        public Authentication AuthenticationLocal { get; }

        /// <summary>
        /// Configuration for local authentication
        /// </summary>
        public Authentication AuthenticationNexus { get; }

        /// <summary>
        /// Configuration for Nexus services
        /// </summary>
        public NexusCapabilityEndpoints NexusCapabilityEndpoints { get; }
    }
}
