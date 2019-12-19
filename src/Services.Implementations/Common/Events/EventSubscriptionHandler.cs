using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Services.Contracts;
using Nexus.Link.Services.Contracts.Events;
using JsonHelper = Nexus.Link.Libraries.Core.Json.JsonHelper;

namespace Nexus.Link.Services.Implementations.Adapter.Events
{
    /// <summary>
    /// Handle the event subscriptions for an adapter
    /// </summary>
    public class EventSubscriptionHandler
    {
        private readonly ConcurrentDictionary<string, EventDelegateInfo> _eventReceiverDelegates = 
            new ConcurrentDictionary<string, EventDelegateInfo>();

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
            var eventDelegateInfo = new EventDelegateInfo
            {
                Type = typeof(T),
                Delegate = eventReceiverDelegateAsync
            };
            var success = _eventReceiverDelegates.TryAdd(key, eventDelegateInfo);
            InternalContract.Require(success, $"The event {key} already has a delegate. Latest delegate ({DelegateLogString(eventReceiverDelegateAsync)}) was ignored.");
            return this;
        }

        // TODO: Refactor and also calling methods

        /// <summary>
        /// Call the registered method for this event
        /// </summary>
        /// <exception cref="FulcrumNotImplementedException"></exception>
        public async Task CallEventReceiverAsync(JObject eventAsJObject)
        {
            var publishableEvent = JsonHelper.SafeDeserializeObject<PublishableEvent>(eventAsJObject);
            try
            {
                InternalContract.RequireNotNull(eventAsJObject, nameof(eventAsJObject));
                FulcrumAssert.IsNotNull(publishableEvent, CodeLocation.AsString(), $"Could not convert to {nameof(PublishableEvent)}: {eventAsJObject}");
                var key = ToKey(publishableEvent);
                var success = _eventReceiverDelegates.TryGetValue(key, out var eventDelegateInfo);
                if (!success)
                {
                    Log.LogWarning($"This adapter received an event that it doesn't subscribe to ({key}).");
                    return;
                }

                FulcrumAssert.IsNotNull(eventDelegateInfo?.Delegate, CodeLocation.AsString(), $"Could not find a delegate for event {eventAsJObject}");
                if (eventDelegateInfo?.Delegate == null) return;

                Log.LogOnLevel(
                    FulcrumApplication.IsInProductionOrProductionSimulation
                        ? LogSeverityLevel.Verbose
                        : LogSeverityLevel.Information,
                    $"Event {publishableEvent.ToLogString()} delegated to {DelegateLogString(eventDelegateInfo.Delegate)}.");
                var typedEvent = JsonHelper.SafeDeserializeObject(eventAsJObject, eventDelegateInfo.Type);
                FulcrumAssert.IsNotNull(typedEvent, CodeLocation.AsString());
                var i = typedEvent as IPublishableEvent;
                FulcrumAssert.IsNotNull(i, CodeLocation.AsString(), $"Could not cast event to {nameof(IPublishableEvent)}.\rEvent: {eventAsJObject}");
                InternalContract.RequireValidated(i, nameof(eventAsJObject));
                await eventDelegateInfo.Delegate(i);
            }
            catch (Exception e)
            {
                Log.LogError(
                    $"Failed to handle event {publishableEvent.Metadata.ToLogString()}\r{e.GetType().FullName}: {e.Message}\r{e}");
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

        private class EventDelegateInfo
        {
            public EventReceiverDelegateAsync<IPublishableEvent> Delegate { get; set; }
            public Type Type { get; set; }
        }
    }
}
