using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.Sdk.UnitTests.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.Services
{
    public class WorkflowParameterServiceTests : WorkflowParameterServiceTestsBase<FulcrumContractException>
    {
        public WorkflowParameterServiceTests()
        :base(new WorkflowParameterService(new ConfigurationTablesMemory()))
        {
        }
    }
}
