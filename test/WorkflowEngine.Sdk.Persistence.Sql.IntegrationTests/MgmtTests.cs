using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Mgmt;
using WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.Support;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests
{
    [Collection("workflow-sdk-tests")]
    public class WorkMgmtTests : WorkflowServiceTests
    {
        public WorkMgmtTests() : base(PersistenceHelper.ConfigurationTables, PersistenceHelper.RuntimeTables)
        {
        }
    }
}
