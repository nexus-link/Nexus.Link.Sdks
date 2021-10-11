using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.UnitTests.Abstract.Services;

namespace WorkflowEngine.UnitTests.Services
{
    public class ActivityParameterServiceTests : ActivityParameterServiceTestsBase<FulcrumContractException>
    {
        public ActivityParameterServiceTests()
        :base(new ActivityParameterService(new ConfigurationTablesMemory()))
        {
        }
    }
}
