using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.ConfigurationTablesTests;
using WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.Support;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests
{
    [Collection("workflow-sdk-tests")]
    public class WorkflowFormTableTests : WorkflowFormTableTestsBase
    {
        public WorkflowFormTableTests() : base(PersistenceHelper.ConfigurationTables, PersistenceHelper.RuntimeTables)
        {
        }
    }

    [Collection("workflow-sdk-tests")]
    public class WorkflowVersionTableTests : WorkflowVersionTableTestsBase
    {
        public WorkflowVersionTableTests() : base(PersistenceHelper.ConfigurationTables, PersistenceHelper.RuntimeTables)
        {
        }
    }

    [Collection("workflow-sdk-tests")]
    public class ActivityFormTableTests : ActivityFormTableTestsBase
    {
        public ActivityFormTableTests() : base(PersistenceHelper.ConfigurationTables, PersistenceHelper.RuntimeTables)
        {
        }
    }

    [Collection("workflow-sdk-tests")]
    public class ActivityVersionTableTests : ActivityVersionTableTestsBase
    {
        public ActivityVersionTableTests() : base(PersistenceHelper.ConfigurationTables, PersistenceHelper.RuntimeTables)
        {
        }
    }
}
