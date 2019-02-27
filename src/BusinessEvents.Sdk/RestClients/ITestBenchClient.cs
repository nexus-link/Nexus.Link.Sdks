using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal interface ITestBenchClient
    {
        Task<PublicationTestResult> Publish(string entity, string @event, int major, int minor, string clientName, JToken payload);
    }
}
