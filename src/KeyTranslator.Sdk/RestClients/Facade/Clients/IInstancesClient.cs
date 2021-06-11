using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.Models;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public interface IInstancesClient
    {
        Task<IDictionary<string, InstancesExistsResult>> InstancesExists(IEnumerable<string> conceptKeys, CancellationToken cancellationToken = default);
    }
}
