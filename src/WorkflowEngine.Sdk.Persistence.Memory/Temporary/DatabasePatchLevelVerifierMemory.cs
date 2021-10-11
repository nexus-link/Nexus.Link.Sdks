using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Temporary
{
    public class DatabasePatchLevelVerifierMemory : IVerifyDatabasePatchLevel
    {
        public Task VerifyDatabasePatchLevel(int sdkPatchLevel, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
