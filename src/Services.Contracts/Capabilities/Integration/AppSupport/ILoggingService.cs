using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport
{
    /// <summary>
    /// Services for BusinessEvents
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Publish an event
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        Task LogAsync(JToken message, CancellationToken token = default(CancellationToken));
    }
}
