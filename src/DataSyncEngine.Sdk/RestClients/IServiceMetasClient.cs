using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.DatasyncEngine.Sdk.RestClients
{
    internal interface IServiceMetasClient : IResourceHealth2
    {
        /// <summary>
        /// Health for the service itself
        /// </summary>
        Task<HealthInfo> GetResourceHealth2Async();
    }
}
