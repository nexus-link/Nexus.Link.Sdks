using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Logging;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Logging
{
    public class LoggingLogic : ILoggingService
    {
        public LoggingLogic()
        {
        }

        /// <inheritdoc />
        public Task LogAsync(JToken message, CancellationToken token = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}
