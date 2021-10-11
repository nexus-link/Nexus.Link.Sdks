using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.UnitTests.Abstract.Services;

namespace WorkflowEngine.UnitTests.Services
{
    public class WorkflowVersionServiceTests : WorkflowVersionServiceTestsBase<FulcrumContractException>
    {
        public WorkflowVersionServiceTests()
        :base(new WorkflowVersionService(new ConfigurationTablesMemory()))
        {
        }
    }
}
