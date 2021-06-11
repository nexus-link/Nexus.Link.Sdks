using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Configurations.Sdk
{
    /// <summary>
    /// A manager that can get the configuration for a specific tenant.
    /// </summary>
    public interface ILeverConfigurationsManager
    {
        /// <summary>
        /// Get a configuration for the specified tenant.
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ILeverConfiguration> GetConfigurationForAsync(Tenant tenant, CancellationToken cancellationToken = default);
        /// <summary>
        /// Clear the cache of tenant configurations.
        /// </summary>
        void ClearCache();
    }
}
