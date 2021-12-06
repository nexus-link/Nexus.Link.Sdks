using Nexus.Link.Capabilities.WorkflowConfiguration.UnitTests.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.Configuration;

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
