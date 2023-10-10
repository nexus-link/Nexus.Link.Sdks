using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal interface IServiceMetasClient : IResourceHealth
    {
        /// <summary>
        /// Get Business Events health for a tenant 
        /// </summary>
        Task<Health> TenantHealthAsync(Tenant tenant, CancellationToken cancellationToken = default);
    }
}
