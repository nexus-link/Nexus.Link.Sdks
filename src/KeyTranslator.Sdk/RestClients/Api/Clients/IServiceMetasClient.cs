using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public interface IServiceMetasClient
    {

        Task<HealthResponse> ServiceHealthAsync(CancellationToken cancellationToken = default);
    }
}
