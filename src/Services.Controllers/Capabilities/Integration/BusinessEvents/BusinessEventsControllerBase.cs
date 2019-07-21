using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents.Model;

namespace Nexus.Link.Services.Controllers.Capabilities.Integration.BusinessEvents
{
    /// <summary>
    /// Service implementation of <see cref="IBusinessEventService"/>
    /// </summary>
    public abstract class BusinessEventsControllerBase : ControllerBase, IBusinessEventService
    {
        protected readonly IBusinessEventsCapability Capability;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capability">The logic layer</param>
        protected BusinessEventsControllerBase(IBusinessEventsCapability capability)
        {
            Capability = capability;
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("")]
        [Authorize(Roles = "business-api-caller")]
        public virtual Task PublishAsync(JToken @event, CancellationToken token = default(CancellationToken))
        {
            ServiceContract.RequireNotNull(@event, nameof(@event));
            ServiceContract.Require(@event.Type == JTokenType.Object, $"The {nameof(@event)} parameter must be a JSON object.");
            var publishableEvent =JsonHelper.SafeDeserializeObject<PublishableEvent>(@event as JObject);
            ServiceContract.RequireNotNull(publishableEvent?.Metadata, nameof(publishableEvent.Metadata));
            ServiceContract.RequireValidated(publishableEvent?.Metadata, nameof(publishableEvent.Metadata));
            return Capability.BusinessEventService.PublishAsync(@event, token);
        }
    }
}
