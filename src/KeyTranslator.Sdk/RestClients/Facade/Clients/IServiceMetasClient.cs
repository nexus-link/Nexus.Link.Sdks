using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public interface IServiceMetasClient
    {
        Task<HealthResponse> ServiceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default);
    }
}
