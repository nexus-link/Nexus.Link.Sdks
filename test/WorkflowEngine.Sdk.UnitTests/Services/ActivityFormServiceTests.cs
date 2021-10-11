using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.UnitTests.Abstract.Services;

namespace WorkflowEngine.UnitTests.Services
{
    public class ActivityFormServiceTests : ActivityFormServiceTestsBase
    {
        public ActivityFormServiceTests()
        :base(new ActivityFormService(new ConfigurationTablesMemory()))
        {
        }
    }
}
