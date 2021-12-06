using Nexus.Link.Capabilities.WorkflowConfiguration.UnitTests.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.Configuration;

namespace WorkflowEngine.Sdk.UnitTests.Services.Configuration
{
    public class ActivityParameterServiceTests : ActivityParameterServiceTestsBase<FulcrumContractException>
    {
        public ActivityParameterServiceTests()
        :base(new ActivityParameterService(new ConfigurationTablesMemory()))
        {
        }
    }
}
