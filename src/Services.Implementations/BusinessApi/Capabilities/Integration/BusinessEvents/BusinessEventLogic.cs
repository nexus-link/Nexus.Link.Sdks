using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Contracts.Events;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.BusinessEvents
{
    /// <inheritdoc />
    public class BusinessEventLogic : IBusinessEventService
    {
        private static Link.BusinessEvents.Sdk.BusinessEvents _businessEventsService;

        /// <inheritdoc />
        public BusinessEventLogic(string serviceBaseUrl, ServiceClientCredentials serviceClient)
        {
            _businessEventsService =
                new Link.BusinessEvents.Sdk.BusinessEvents(serviceBaseUrl, FulcrumApplication.Setup.Tenant, serviceClient);
        }

        /// <inheritdoc />
        public Task PublishAsync(JToken eventAsJson, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(eventAsJson, nameof(eventAsJson));
            var @event = JsonHelper.SafeDeserializeObject<PublishableEvent>(eventAsJson.ToString(Formatting.None));
            InternalContract.RequireNotNull(@event, nameof(@event));
            InternalContract.RequireNotNull(@event?.Metadata, nameof(@event.Metadata));
            InternalContract.RequireValidated(@event?.Metadata, nameof(@event.Metadata));
            FulcrumAssert.IsNotNullOrWhiteSpace(FulcrumApplication.Context?.ClientPrincipal?.Identity?.Name, CodeLocation.AsString());
            var metadata = @event?.Metadata;
            if (metadata == null) throw new FulcrumAssertionFailedException("Metadata was unexpectedly null.");
            return _businessEventsService.PublishAsync(
                metadata?.EntityName,
                metadata?.EventName,
                metadata.MajorVersion,
                metadata.MinorVersion, 
                FulcrumApplication.Context?.ClientPrincipal?.Identity?.Name, 
                eventAsJson);
        }
    }
}
