using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.ConfigurationTablesTests;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.RuntimeTablesTests;
using WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.Support;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests
{
    [Collection("workflow-sdk-tests")]
    public class WorkflowInstanceTableTests : WorkflowInstanceTableTestsBase
    {
        public WorkflowInstanceTableTests() : base(PersistenceHelper.ConfigurationTables, PersistenceHelper.RuntimeTables)
        {
        }
    }

    [Collection("workflow-sdk-tests")]
    public class ActivityInstanceTableTests : ActivityInstanceTableTestsBase
    {
        public ActivityInstanceTableTests() : base(PersistenceHelper.ConfigurationTables, PersistenceHelper.RuntimeTables)
        {
        }
    }
}
