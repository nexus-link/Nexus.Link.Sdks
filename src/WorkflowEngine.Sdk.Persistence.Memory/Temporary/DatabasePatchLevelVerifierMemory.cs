using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Temporary
{
    public class DatabasePatchLevelVerifierMemory : IVerifyDatabasePatchLevel
    {
        public Task VerifyDatabasePatchLevel(Tenant tenant, int sdkPatchLevel, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
