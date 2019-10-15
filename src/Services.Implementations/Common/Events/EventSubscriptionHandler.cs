using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Services.Contracts;
using Nexus.Link.Services.Contracts.Events;

namespace Nexus.Link.Services.Implementations.Adapter.Events
{
    /// <summary>
    /// Handle the event subscriptions for an adapter
    /// </summary>
    public class EventSubscriptionHandler
    {
        private readonly ConcurrentDictionary<string, EventReceiverDelegateAsync<IPublishableEvent>> _eventReceiverDelegates = new ConcurrentDictionary<string,EventReceiverDelegateAsync<IPublishableEvent>>();

        /// <summary>
        /// A delegate for receiving events
        /// </summary>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        public delegate Task EventReceiverDelegateAsync<in T>(T @event, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// True if any subscription has been added to this subscription handler.
        /// </summary>
        public bool HasSubscriptions => _eventReceiverDelegates.Any();

        /// <summary>
        /// Add another <see cref="EventReceiverDelegateAsync{T}"/>
        /// </summary>
        public EventSubscriptionHandler Add<T>(EventReceiverDelegateAsync<T> eventReceiverDelegateAsync)
        where T : IPublishableEvent, new()
        {
            var item = new T();
            var key = ToKey(item);
            var d = eventReceiverDelegateAsync as EventReceiverDelegateAsync<IPublishableEvent>;
            var success = _eventReceiverDelegates.TryAdd(key, d);
            InternalContract.Require(success, $"The event {key} has already been added");
            return this;
        }

        /// <summary>
        /// Call the registered method for this event
        /// </summary>
        /// <param name="event"></param>
        /// <exception cref="FulcrumNotImplementedException"></exception>
        public async Task CallEventReceiverAsync<T>(T @event)
            where T : IPublishableEvent, new()
        {
            var key = ToKey(@event);
            var success = _eventReceiverDelegates.TryGetValue(key, out var eventReceiverDelegate);
            if (!success)
            {
                Log.LogWarning($"This adapter received an event that it doesn't subscribe to ({key}).");
                return;
            }

            Log.LogOnLevel(
                FulcrumApplication.IsInProductionOrProductionSimulation ? LogSeverityLevel.Verbose : LogSeverityLevel.Information,
                $"Event {@event.ToLogString()} delegated to {DelegateLogString(eventReceiverDelegate)}.");
            await eventReceiverDelegate(@event);
        }

        private string DelegateLogString(EventReceiverDelegateAsync<IPublishableEvent> d)
        {
            var methodInfo = d.GetMethodInfo();
            if (methodInfo == null) return "Unknown delegate";
            return $"{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}()";
        }

        private static string ToKey<T>(T item) where T : IPublishableEvent, new()
        {
            return $"{item.Metadata.EntityName}|{item.Metadata.EventName}|{item.Metadata.MajorVersion}";
        }
    }
}
