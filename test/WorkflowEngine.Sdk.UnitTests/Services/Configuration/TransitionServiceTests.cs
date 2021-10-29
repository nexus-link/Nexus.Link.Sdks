using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.Configuration;
using WorkflowEngine.Sdk.UnitTests.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.Services.Configuration
{
    public class TransitionServiceTests : TransitionServiceTestsBase
    {
        public TransitionServiceTests()
        :base(new TransitionService(new ConfigurationTablesMemory()))
        {
        }
    }
}
