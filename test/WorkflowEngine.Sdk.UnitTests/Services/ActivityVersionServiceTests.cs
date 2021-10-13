using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.Sdk.UnitTests.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.Services
{
    public class ActivityVersionServiceTests : ActivityVersionServiceTestsBase
    {
        public ActivityVersionServiceTests()
        :base(new ActivityVersionService(new ConfigurationTablesMemory()))
        {
        }
    }
}
