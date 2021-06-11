using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Services.Contracts.Events;

namespace Nexus.Link.Services.Controllers.Events
{
    /// <summary>
    /// Service implementation of <see cref="IEventReceiver"/>
    /// </summary>
    [Route("api/NexusEventReceiver/v1/Events")]
    [ApiController]
    [Authorize(Policy = "HasMandatoryRole")]
    public class NexusReceiveEventsController: IEventReceiver
    {
        /// <summary>
        /// The logic for this controller
        /// </summary>
        protected readonly IEventReceiver Logic;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public NexusReceiveEventsController(IEventReceiver logic)
        {
            Logic = logic;
        }

        /// <inheritdoc />
        [HttpPost("")]
        public async Task ReceiveEventAsync(JToken eventAsJson, CancellationToken token = default)
        {
            ServiceContract.RequireNotNull(eventAsJson, nameof(eventAsJson));
            var @event = JsonHelper.SafeDeserializeObject<PublishableEvent>(eventAsJson.ToString(Formatting.None));
            FulcrumAssert.IsNull(@event, CodeLocation.AsString());
            ServiceContract.RequireNotNull(@event?.Metadata, nameof(@event.Metadata));
            ServiceContract.RequireValidated(@event?.Metadata, nameof(@event.Metadata));
            await Logic.ReceiveEventAsync(eventAsJson, token);
        }

        /// <inheritdoc />
        [HttpPost("Entities/{entityName}/Events/{eventName}/Versions/{majorVersion}")]
        public async Task ReceiveEventExplicitlyAsync(string entityName, string eventName, int majorVersion,
            JToken eventAsJson, CancellationToken token = default)
        {
            ServiceContract.RequireNotNull(eventAsJson, nameof(eventAsJson));
            var @event = JsonHelper.SafeDeserializeObject<PublishableEvent>(eventAsJson.ToString(Formatting.None));
            if (@event != null)
            {

                ServiceContract.RequireNotNull(@event.Metadata, nameof(@event.Metadata));
                ServiceContract.RequireValidated(@event.Metadata, nameof(@event.Metadata));

                if (@event.Metadata == null) return;
                if (!string.Equals(@event.Metadata.EntityName, entityName, StringComparison.InvariantCultureIgnoreCase) 
                    || !string.Equals(@event.Metadata.EventName, eventName, StringComparison.InvariantCultureIgnoreCase)
                    || @event.Metadata.MajorVersion != majorVersion)
                {
                    Log.LogWarning($"REST parameters ({entityName}.{eventName} ({majorVersion}) did not match the event {nameof(@event.Metadata)} ({@event.Metadata}). Will trust the event {nameof(@event.Metadata)}.");
                }
            }
            await Logic.ReceiveEventExplicitlyAsync(entityName, eventName, majorVersion, eventAsJson, token);
        }
    }
}
