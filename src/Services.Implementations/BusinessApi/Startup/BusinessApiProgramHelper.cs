using Microsoft.Extensions.Configuration;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Startup;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.AppSupport;
using Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration;
using Nexus.Link.Services.Implementations.Startup;
#if NETCOREAPP

namespace Nexus.Link.Services.Implementations.BusinessApi.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public static class BusinessApiProgramHelper
    {
        /// <summary>
        /// Use this in Program.cs to make essential configurations.
        /// </summary>
        /// <param name="builder"></param>
        public static void AddConfiguration(IConfigurationBuilder builder)
        {
            var configuration = new BusinessApiConfiguration(builder.Build());
            ProgramHelper.AddConfiguration(configuration.Configuration);
            var tokenRefresher =
                NexusAuthenticationManager.CreateTokenRefresher(
                    FulcrumApplication.Setup.Tenant,
                    configuration.AuthenticationNexus.Endpoint, 
                    configuration.AuthenticationNexus.Credentials);
            var httpSender = new HttpSender(configuration.NexusCapabilityEndpoints.AppSupport, tokenRefresher.GetServiceClient());
            var appSupportCapability = new AppSupportCapability(null, httpSender);
            builder.Add(new NexusConfigurationSource(configuration.AuthenticationLocal.ClientId, appSupportCapability.ConfigurationService));
        }
    }
}
#endif