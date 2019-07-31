using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Logging;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Logging
{
    /// <inheritdoc cref="ILoggingService" />
    public class LoggingRestService : RestClientBase, ILoggingService
    {
        public LoggingRestService(string baseUrl, ServiceClientCredentials credentials)
            :base($"{baseUrl}/Logs", credentials)
        {
        }

        /// <inheritdoc />
        public Task LogAsync(JToken message, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(message, nameof(message));
            return RestClient.PostNoResponseContentAsync<JToken>("", message, null, token);
        }
    }
}
