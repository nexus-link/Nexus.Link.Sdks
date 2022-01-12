using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nexus.Link.Services.Contracts.Events
{
    /// <summary>
    /// Needs to be implemented by an adapter that subscribes to events
    /// </summary>
    public interface IEventReceiver : Libraries.Core.Platform.Services.IControllerInjector
    {
        /// <summary>
        /// This method will be called when an event is available
        /// </summary>
        /// <param name="eventAsJson">The event</param>
        /// <param name="cancellationToken"></param>
        Task ReceiveEventAsync(JToken eventAsJson, CancellationToken cancellationToken = default);

        /// <summary>
        /// This method will be called when an event is available and it was made available by explicitly stating meta
        /// information about the event.
        /// </summary>
        Task ReceiveEventExplicitlyAsync(string entityName, string eventName, int majorVersion,
            JToken eventAsJson, CancellationToken cancellationToken = default);
    }
}