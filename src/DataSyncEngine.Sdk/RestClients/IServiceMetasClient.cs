using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Health.Model;

namespace Nexus.Link.DatasyncEngine.Sdk.RestClients
{
    public interface IServiceMetasClient : IResourceHealth2
    {
        /// <summary>
        /// Health for the service itself
        /// </summary>
        Task<HealthInfo> GetResourceHealth2Async(CancellationToken cancellationToken = default);
    }
}
