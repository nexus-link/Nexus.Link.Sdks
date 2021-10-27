using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Temporary
{
    [Obsolete("You are discouraged to use the Memory implementation of Nexus Workflows. Instead use the Rest Clients implementation.")]
    public class DatabasePatchLevelVerifierMemory : IVerifyDatabasePatchLevel
    {
        public Task VerifyDatabasePatchLevel(int sdkPatchLevel, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
