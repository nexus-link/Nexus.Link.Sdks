using Nexus.Link.Capabilities.WorkflowState.UnitTests.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;

namespace WorkflowEngine.Sdk.UnitTests.Services.State
{
    public class WorkflowInstanceServiceTests : WorkflowInstanceServiceTestsBase<FulcrumContractException>
    {
        public WorkflowInstanceServiceTests()
        :base(new WorkflowInstanceService(new RuntimeTablesMemory()))
        {
        }
    }
}
