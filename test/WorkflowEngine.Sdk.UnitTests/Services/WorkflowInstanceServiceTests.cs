using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.UnitTests.Abstract.Services;

namespace WorkflowEngine.UnitTests.Services
{
    public class WorkflowInstanceServiceTests : WorkflowInstanceServiceTestsBase<FulcrumContractException>
    {
        public WorkflowInstanceServiceTests()
        :base(new WorkflowInstanceService(new RuntimeTablesMemory()))
        {
        }
    }
}
