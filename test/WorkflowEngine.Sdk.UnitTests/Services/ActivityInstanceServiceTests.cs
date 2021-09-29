using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.UnitTests.Abstract.Services;

namespace WorkflowEngine.UnitTests.Services
{
    public class ActivityInstanceServiceTests : ActivityInstanceServiceTestsBase
    {
        public ActivityInstanceServiceTests()
        :base(new ActivityInstanceService(new RuntimeTablesMemory()))
        {
        }
    }
}
