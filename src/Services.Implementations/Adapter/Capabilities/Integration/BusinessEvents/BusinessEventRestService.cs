using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.BusinessEvents
{
    /// <inheritdoc cref="IBusinessEventService" />
    public class BusinessEventRestService : RestClientBase, IBusinessEventService
    {
        /// <inheritdoc cref="IBusinessEventService" />
        public BusinessEventRestService(string baseUrl, HttpClient httpClient, ServiceClientCredentials credentials)
        :base($"{baseUrl}/BusinessEvents", httpClient, credentials)
        {
        }

        /// <inheritdoc />
        public Task PublishAsync(JToken @event, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(@event, nameof(@event));
            return RestClient.PostNoResponseContentAsync<JToken>("", @event, null, token);
        }
    }
}