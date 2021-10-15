using Nexus.Link.Capabilities.WorkflowMgmt.UnitTests.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.Sdk.UnitTests.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.Services
{
    public class ActivityInstanceServiceTests : ActivityInstanceServiceTestsBase
    {
        public ActivityInstanceServiceTests()
        :base(new ActivityInstanceService(new RuntimeTablesMemory()))
        {
        }
    }
}
