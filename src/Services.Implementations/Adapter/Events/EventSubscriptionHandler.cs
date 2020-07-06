﻿using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Services.Contracts.Events;

namespace Nexus.Link.Services.Implementations.Adapter.Events
{
    /// <summary>
    /// Handle the event subscriptions for an adapter
    /// </summary>
    public class EventSubscriptionHandler
    {
        private readonly ConcurrentDictionary<string, EventReceiverDelegate<IPublishableEvent>> _eventReceiverDelegates = new ConcurrentDictionary<string,EventReceiverDelegate<IPublishableEvent>>();

        /// <summary>
        /// A delegate for receiving events
        /// </summary>
        /// <param name="event"></param>
        /// <typeparam name="T"></typeparam>
        public delegate Task EventReceiverDelegate<in T>(T @event);

        /// <summary>
        /// Add another <see cref="EventReceiverDelegate{T}"/>
        /// </summary>
        public EventSubscriptionHandler Add<T>(EventReceiverDelegate<T> eventReceiverDelegate)
        where T : IPublishableEvent, new()
        {
            var item = new T();
            var key = ToKey(item);
            var d = eventReceiverDelegate as EventReceiverDelegate<IPublishableEvent>;
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

            await eventReceiverDelegate(@event);
        }

        private static string ToKey<T>(T item) where T : IPublishableEvent, new()
        {
            return $"{item.Metadata.EntityName}|{item.Metadata.EventName}|{item.Metadata.MajorVersion}";
        }
    }
}