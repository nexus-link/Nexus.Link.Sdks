using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Services.Contracts.Events;

namespace Nexus.Link.Services.Implementations.Adapter.Events
{
    internal class EventReceiverLogic : IEventReceiver
    {
        private readonly EventSubscriptionHandler _subscriptionHandler;

        public EventReceiverLogic(EventSubscriptionHandler subscriptionHandler)
        {
            _subscriptionHandler = subscriptionHandler;
        }
        /// <inheritdoc />
        public async Task ReceiveEvent(JToken eventAsJson, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotNull(eventAsJson, nameof(eventAsJson));
            InternalContract.Require(eventAsJson.Type == JTokenType.Object, $"The {nameof(eventAsJson)} parameter must be a JSON object.");
            var publishableEvent =JsonHelper.SafeDeserializeObject<PublishableEvent>(eventAsJson as JObject);
            InternalContract.RequireNotNull(publishableEvent?.Metadata, nameof(publishableEvent.Metadata));
            InternalContract.RequireValidated(publishableEvent?.Metadata, nameof(publishableEvent.Metadata));
            await _subscriptionHandler.CallEventReceiverAsync(publishableEvent);
        }
    }
}