using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents
{
    /// <summary>
    /// Service for BusinessEvents
    /// </summary>
    public interface IBusinessEventService
    {
        /// <summary>
        /// Publish an event
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        Task PublishAsync(JToken @event, CancellationToken token = default);
    }
}
