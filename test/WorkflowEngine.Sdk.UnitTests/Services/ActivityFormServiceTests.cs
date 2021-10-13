using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.Sdk.UnitTests.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.Services
{
    public class ActivityFormServiceTests : ActivityFormServiceTestsBase
    {
        public ActivityFormServiceTests()
        :base(new ActivityFormService(new ConfigurationTablesMemory()))
        {
        }
    }
}
