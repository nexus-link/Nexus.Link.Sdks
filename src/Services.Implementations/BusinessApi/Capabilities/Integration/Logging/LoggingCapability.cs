using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Logging;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Logging
{
    public class LoggingCapability : ILoggingCapability
    {
        public LoggingCapability(IAsyncLogger logger)
        {
            LoggingService = new LoggingLogic(logger);
        }

        /// <inheritdoc />
        public ILoggingService LoggingService { get; }
    }
}
