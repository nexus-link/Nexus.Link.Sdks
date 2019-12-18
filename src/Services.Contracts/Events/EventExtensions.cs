using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Contracts.Events
{
    // TODO: This class does not belong in the Services.Contracts library, but it was put here to avoid introducing a new library with only this file.
    /// <summary>
    /// Extensions for events
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// This property must be initialized in the Startup.cs.
        /// </summary>
        public static IBusinessEventService BusinessEventService { get; set; }

        /// <summary>
        /// See <see cref="IBusinessEventService.PublishAsync"/>.
        /// </summary>
        public static Task PublishAsync(this IPublishableEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.Require(BusinessEventService != null, $"Publish failed, prerequisite was not fulfilled: The Business API SDK must set the {typeof(EventExtensions).FullName}.{nameof(BusinessEventService)} property.");
            if (BusinessEventService == null) return Task.CompletedTask;
            var jToken = JToken.FromObject(@event);
            return BusinessEventService.PublishAsync(jToken, cancellationToken);
        }
    }
}