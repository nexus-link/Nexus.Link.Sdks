using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Logging;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Logging
{
    public class LoggingCapability : ILoggingCapability
    {
        public LoggingCapability(ITokenRefresherWithServiceClient tokenRefresher)
        {
            LoggingService = new LoggingLogic();
        }

        /// <inheritdoc />
        public ILoggingService LoggingService { get; }
    }
}
