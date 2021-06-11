using System.Threading;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Queue.Model;
using System.Threading.Tasks;

namespace Nexus.Link.Logger.Sdk.Helpers
{
    public interface ILogQueueHelper<T>
    {
        Task<(bool HasStorageQueue, IWritableQueue<T> WritableQueue)> TryGetQueueAsync(Tenant tenant, CancellationToken cancellationToken = default);
    }
}