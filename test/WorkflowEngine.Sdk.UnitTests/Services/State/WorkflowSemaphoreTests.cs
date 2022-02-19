using Nexus.Link.Capabilities.WorkflowState.UnitTests.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;

namespace WorkflowEngine.Sdk.UnitTests.Services.State
{
    public class WorkflowSemaphoreTests : WorkflowSemaphoreTestBase
    {
        public WorkflowSemaphoreTests()
        : base(new WorkflowSemaphoreService(new RuntimeTablesMemory()))
        {
        }
    }
}
