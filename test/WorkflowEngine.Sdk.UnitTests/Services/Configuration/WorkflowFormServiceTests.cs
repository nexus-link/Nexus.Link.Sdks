using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.Configuration;
using WorkflowEngine.Sdk.UnitTests.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.Services.Configuration
{
    public class WorkflowFormServiceTests : WorkflowFormServiceTestsBase
    {
        public WorkflowFormServiceTests()
        :base(new WorkflowFormService(new ConfigurationTablesMemory()))
        {
        }
    }
}
