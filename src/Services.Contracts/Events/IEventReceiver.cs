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
        Task ReceiveEventAsync(JToken eventAsJson, CancellationToken token = default(CancellationToken));

        Task ReceiveEventExplicitlyAsync(string entityName, string eventName, int majorVersion,
            JToken eventAsJson, CancellationToken token = default(CancellationToken));
    }
}