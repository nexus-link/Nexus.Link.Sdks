using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.BusinessEvents
{
    /// <inheritdoc cref="IBusinessEventService" />
    public class BusinessEventRestService : RestClientBase, IBusinessEventService
    {
        /// <inheritdoc cref="IBusinessEventService" />
        public BusinessEventRestService(IHttpSender httpSender)
        :base(httpSender)
        {
        }

        /// <inheritdoc />
        public Task PublishAsync(JToken @event, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(@event, nameof(@event));
            return RestClient.PostNoResponseContentAsync("", @event, null, token);
        }
    }
}