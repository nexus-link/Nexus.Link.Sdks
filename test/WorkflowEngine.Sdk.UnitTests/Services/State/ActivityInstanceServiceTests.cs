using Moq;
using Nexus.Link.AsyncManager.Sdk.RestClients;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;
using WorkflowEngine.Sdk.UnitTests.Abstract.Services;

namespace WorkflowEngine.Sdk.UnitTests.Services.State
{
    public class ActivityInstanceServiceTests : ActivityInstanceServiceTestsBase
    {
        public ActivityInstanceServiceTests()
        :base(new ActivityInstanceService(new RuntimeTablesMemory(), new AsyncRequestMgmtRestClients(Mock.Of<IHttpSender>())))
        {
        }
    }
}
