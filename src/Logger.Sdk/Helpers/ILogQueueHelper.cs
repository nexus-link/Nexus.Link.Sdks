using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Queue.Model;

namespace Nexus.Link.Logger.Sdk.Helpers
{
    public interface ILogQueueHelper<T>
    {
        bool TryGetQueue(Tenant tenant, out IWritableQueue<T> queue);
    }
}