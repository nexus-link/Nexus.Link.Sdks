using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Services.Contracts;
using Nexus.Link.Services.Contracts.Events;

namespace Nexus.Link.Services.Implementations.Adapter.Events
{
    /// <summary>
    /// Handle the event subscriptions for an adapter
    /// </summary>
    public class EventSubscriptionHandler
    {
        private readonly ConcurrentDictionary<string, EventReceiverDelegateAsync<IPublishableEvent>> _eventReceiverDelegates = 
            new ConcurrentDictionary<string, EventReceiverDelegateAsync<IPublishableEvent>>();

        /// <summary>
        /// A delegate for receiving events
        /// </summary>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        public delegate Task EventReceiverDelegateAsync<in T>(T @event, CancellationToken cancellationToken = default(CancellationToken))
            where T : IPublishableEvent;

        /// <summary>
        /// True if any subscription has been added to this subscription handler.
        /// </summary>
        public bool HasSubscriptions => _eventReceiverDelegates.Any();

        /// <summary>
        /// Add another <see cref="EventReceiverDelegateAsync{T}"/>
        /// </summary>
        public EventSubscriptionHandler Add<T>(EventReceiverDelegateAsync<IPublishableEvent> eventReceiverDelegateAsync)
            where T : IPublishableEvent, new()
        {
            InternalContract.RequireNotNull(eventReceiverDelegateAsync, nameof(eventReceiverDelegateAsync));
            var eventExample = new T();
            var key = ToKey(eventExample);
            var success = _eventReceiverDelegates.TryAdd(key, eventReceiverDelegateAsync);
            InternalContract.Require(success, $"The event {key} already has a delegate. Latest delegate ({DelegateLogString(eventReceiverDelegateAsync)}) was ignored.");
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
            InternalContract.RequireNotNull(@event, nameof(@event));
            var key = ToKey(@event);
            var success = _eventReceiverDelegates.TryGetValue(key, out var eventReceiverDelegateAsync);
            if (!success)
            {
                Log.LogWarning($"This adapter received an event that it doesn't subscribe to ({key}).");
                return;
            }
            FulcrumAssert.IsNotNull(eventReceiverDelegateAsync, CodeLocation.AsString());

            Log.LogOnLevel(
                FulcrumApplication.IsInProductionOrProductionSimulation ? LogSeverityLevel.Verbose : LogSeverityLevel.Information,
                $"Event {@event.ToLogString()} delegated to {DelegateLogString(eventReceiverDelegateAsync)}.");
            var asyncDelegate = eventReceiverDelegateAsync as EventReceiverDelegateAsync<IPublishableEvent>;
            FulcrumAssert.IsNotNull(asyncDelegate, CodeLocation.AsString());
            if (asyncDelegate == null) return;
            try
            {
                await asyncDelegate(@event);
            }
            catch (Exception e)
            {
                Log.LogError($"Failed to handle event {@event.Metadata.ToLogString()}\r{e.GetType().FullName}: {e.Message}");
            }
        }

        private string DelegateLogString(object d)
        {
            if (d == null) return "NULL delegate";
            if (!(d is EventReceiverDelegateAsync<IPublishableEvent> asyncDelegate)) return $"(FAILED to cast {d.GetType().FullName})";
            var methodInfo = asyncDelegate.GetMethodInfo();
            return methodInfo == null ? "Unknown delegate" : $"{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}()";
        }

        private static string ToKey<T>(T item) where T : IPublishableEvent
        {
            return $"{item.Metadata.EntityName}|{item.Metadata.EventName}|{item.Metadata.MajorVersion}";
        }
    }
}
