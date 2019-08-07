using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.AppSupport
{
    /// <inheritdoc />
    public class AppSupportCapability : IAppSupportCapability
    {
        /// <summary>
        /// The HttpClient for all AppSupport services
        /// </summary>
        protected static HttpClient HttpClient { get; private set; }
        private static readonly object ClassLock = new object();
        
        /// <inheritdoc />
        public AppSupportCapability(IAsyncLogger logger, string baseUri, ServiceClientCredentials credentials)
        {
            lock (ClassLock)
            {
                if (HttpClient == null)
                {
                    HttpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());
                }
            }
            LoggingService = new LoggingLogic(logger);
            var url = $"{baseUri}/api/v1/{FulcrumApplication.Setup.Tenant.Organization}/{FulcrumApplication.Setup.Tenant.Environment}";
            ConfigurationService = new ConfigurationLogic(url, HttpClient, credentials);
        }

        /// <inheritdoc />
        public IConfigurationService ConfigurationService { get; }

        /// <inheritdoc />
        public ILoggingService LoggingService { get; }
    }
}
