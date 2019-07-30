using Microsoft.Rest;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Logging;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Logging
{
    public class LoggingCapability : ILoggingCapability
    {
        public LoggingCapability(string baseUrl, ServiceClientCredentials credentials)
        {
            LoggingService = new LoggingRestService(baseUrl, credentials);
        }

        /// <inheritdoc />
        public ILoggingService LoggingService { get; }
    }
}
