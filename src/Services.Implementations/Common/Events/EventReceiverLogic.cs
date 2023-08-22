using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Services.Contracts.Events;

namespace Nexus.Link.Services.Implementations.Common.Events
{
    internal class EventReceiverLogic : IEventReceiver
    {
        private readonly EventSubscriptionHandler _subscriptionHandler;

        public EventReceiverLogic(EventSubscriptionHandler subscriptionHandler)
        {
            _subscriptionHandler = subscriptionHandler;
        }
        /// <inheritdoc />
        public async Task ReceiveEventAsync(JToken eventAsJson, CancellationToken cancellationToken = default)
        {
            Log.LogOnLevel(
                FulcrumApplication.IsInProductionOrProductionSimulation ? LogSeverityLevel.Verbose : LogSeverityLevel.Information,
                eventAsJson.ToString(Formatting.Indented));
            InternalContract.RequireNotNull(eventAsJson, nameof(eventAsJson));
            InternalContract.Require(eventAsJson.Type == JTokenType.Object, $"The {nameof(eventAsJson)} parameter must be a JSON object.");
            var publishableEvent =JsonHelper.SafeDeserializeObject<PublishableEvent>(eventAsJson as JObject);
            FulcrumAssert.IsNotNull(publishableEvent, CodeLocation.AsString());
            InternalContract.RequireNotNull(publishableEvent?.Metadata, nameof(publishableEvent.Metadata));
            InternalContract.RequireValidated(publishableEvent?.Metadata, nameof(publishableEvent.Metadata));

            await _subscriptionHandler.CallEventReceiverAsync(eventAsJson as JObject, cancellationToken);
        }

        /// <inheritdoc />
        public async Task ReceiveEventExplicitlyAsync(string entityName, string eventName, int majorVersion,
            JToken eventAsJson,
            CancellationToken cancellationToken = default)
        {
            Log.LogOnLevel(
                FulcrumApplication.IsInProductionOrProductionSimulation ? LogSeverityLevel.Verbose : LogSeverityLevel.Information,
                eventAsJson.ToString(Formatting.Indented));
            InternalContract.RequireNotNull(eventAsJson, nameof(eventAsJson));
            InternalContract.Require(eventAsJson.Type == JTokenType.Object,
                $"The {nameof(eventAsJson)} parameter must be a JSON object.");
            var eventAsJObject = eventAsJson as JObject;
            FulcrumAssert.IsNotNull(eventAsJObject, CodeLocation.AsString());
            var publishableEvent = JsonHelper.SafeDeserializeObject<PublishableEvent>(eventAsJObject);
            if (publishableEvent == null)
            {
                var metadata = new EventMetadata(entityName, eventName, majorVersion, 0);
                var metadataAsJObject = JObject.FromObject(metadata);
                eventAsJObject?.Add("Metadata", metadataAsJObject);
                publishableEvent = JsonHelper.SafeDeserializeObject<PublishableEvent>(eventAsJObject);
                FulcrumAssert.IsNotNull(publishableEvent, CodeLocation.AsString());
            }

            InternalContract.RequireNotNull(publishableEvent?.Metadata, nameof(publishableEvent.Metadata));
            InternalContract.RequireValidated(publishableEvent?.Metadata, nameof(publishableEvent.Metadata));
            if (publishableEvent?.Metadata == null) return;

            if (!string.Equals(publishableEvent.Metadata.EntityName, entityName,
                    StringComparison.InvariantCultureIgnoreCase)
                || !string.Equals(publishableEvent.Metadata.EventName, eventName,
                    StringComparison.InvariantCultureIgnoreCase)
                || publishableEvent.Metadata.MajorVersion != majorVersion)
            {
                Log.LogWarning(
                    $"REST parameters ({entityName}.{eventName} ({majorVersion})"
                    + $" did not match the event {nameof(publishableEvent.Metadata)} ({publishableEvent.Metadata})."
                    + $" Will trust the event {nameof(publishableEvent.Metadata)}.");
            }

            await _subscriptionHandler.CallEventReceiverAsync(eventAsJson as JObject, cancellationToken);
        }
    }
}