using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Decoupling;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Logging;

namespace Nexus.Link.Services.Implementations.Integration.Logging
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
