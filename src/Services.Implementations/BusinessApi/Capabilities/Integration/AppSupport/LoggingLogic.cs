using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.AppSupport
{
    /// <inheritdoc />
    public class LoggingLogic : ILoggingService
    {
        private readonly IAsyncLogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public LoggingLogic(IAsyncLogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public Task LogAsync(JToken message, CancellationToken token = default)
        {
            var logRecord = message.ToObject<LogRecord>();
            return _logger.LogAsync(logRecord, token);
        }
    }
}
