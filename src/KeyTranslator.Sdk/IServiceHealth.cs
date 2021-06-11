using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk
{
    /// <summary>
    /// Interface for inspecting the service health
    /// </summary>
    public interface IServiceHealth
    {
        /// <summary>
        /// Get the current service health
        /// </summary>
        Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default);
    }
}