using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.AppSupport
{
    /// <inheritdoc />
    public class AppSupportCapability : IAppSupportCapability
    {
        
        /// <inheritdoc />
        public AppSupportCapability(string baseUrl, HttpClient httpClient, ServiceClientCredentials credentials)
        {
            if (FulcrumApplication.IsInDevelopment)
            {

            }
            else
            {
                LoggingService = new LoggingRestService(baseUrl, httpClient, credentials);
                ConfigurationService = new ConfigurationRestService(baseUrl, httpClient, credentials);
            }
        }

        /// <inheritdoc />
        public IConfigurationService ConfigurationService { get; }

        /// <inheritdoc />
        public ILoggingService LoggingService { get; }
    }
}
