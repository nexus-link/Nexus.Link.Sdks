using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal interface ITestBenchClient
    {
        Task<PublicationTestResult> PublishAsync(string entity, string @event, int major, int minor, string clientName, JToken payload, CancellationToken cancellationToken = default);
    }
}
