using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.UnitTests.Abstract.Services;

namespace WorkflowEngine.UnitTests.Services
{
    public class WorkflowFormServiceTests : WorkflowFormServiceTestsBase
    {
        public WorkflowFormServiceTests()
        :base(new WorkflowFormService(new ConfigurationTablesMemory()))
        {
        }
    }
}
