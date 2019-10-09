using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
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
                LoggingService = new LoggingService_NotImplemented();
                ConfigurationService = new ConfigurationRestService($"{baseUrl}/Configurations", httpClient, credentials);
            }
        }

        /// <inheritdoc />
        public IConfigurationService ConfigurationService { get; }

        /// <inheritdoc />
        public ILoggingService LoggingService { get; }

        // ReSharper disable once InconsistentNaming
        private class LoggingService_NotImplemented : ILoggingService
        {
            public LoggingService_NotImplemented()
            {
            }

            /// <inheritdoc />
            public Task LogAsync(JToken message, CancellationToken token = new CancellationToken())
            {
                throw new FulcrumNotImplementedException();
            }
        }
    }
}
