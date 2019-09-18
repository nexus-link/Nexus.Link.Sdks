using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Contracts.Events;

namespace Nexus.Link.Services.Implementations.Adapter.Events
{
    /// <summary>
    /// Extensions for events
    /// </summary>
    public static class Extensions
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
            InternalContract.Require(BusinessEventService != null, $"The Business API SDK must set {typeof(Extensions).FullName}.{nameof(BusinessEventService)} property.");
            if (BusinessEventService == null) return Task.CompletedTask;
            var jToken = JToken.FromObject(@event);
            return BusinessEventService.PublishAsync(jToken, cancellationToken);
        }
    }
}