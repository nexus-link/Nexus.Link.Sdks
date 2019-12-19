using Microsoft.Extensions.Configuration;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Startup;
using Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration;
using Nexus.Link.Services.Implementations.Adapter.Startup.Configuration;
using Nexus.Link.Services.Implementations.Startup;

namespace Nexus.Link.Services.Implementations.Adapter.Startup
{
    /// <summary>
    /// Helper class for updating the configuration with data from the configuration DB.
    /// </summary>
    public static class NexusAdapterProgramHelper
    {
        /// <summary>
        /// Use this in Program.cs to update the configuration with data from the configuration DB.
        /// </summary>
        public static void AddConfiguration(IConfigurationBuilder builder)
        {
            var configuration = new AdapterConfiguration(builder.Build());
            ProgramHelper.AddConfiguration(configuration.Configuration);
            if (FulcrumApplication.IsInDevelopment) return;
            var integrationCapability = new IntegrationCapability($"{configuration.BusinessApiEndpoint}/api/Integration/v1", configuration.BusinessApiCredentials);
            builder.Add(new NexusConfigurationSource(configuration.BusinessApiClientId, integrationCapability.AppSupport.ConfigurationService));
        }
    }
}