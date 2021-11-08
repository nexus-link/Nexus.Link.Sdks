using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.Configuration;
using WorkflowEngine.Sdk.UnitTests.Abstract.Services;

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