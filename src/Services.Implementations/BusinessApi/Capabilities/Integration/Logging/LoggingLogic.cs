using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Logging;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Logging
{
    public class LoggingLogic : ILoggingService
    {
        private readonly IAsyncLogger _logger;

        public LoggingLogic(IAsyncLogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public Task LogAsync(JToken message, CancellationToken token = default(CancellationToken))
        {
            var logRecord = message.ToObject<LogRecord>();
            return _logger.LogAsync(logRecord);
        }
    }
}
