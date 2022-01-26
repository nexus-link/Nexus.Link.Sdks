using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.RuntimeTablesTests;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;

namespace WorkflowEngine.Sdk.Persistence.Memory.UnitTests
{
    public class WorkflowInstanceTableTests : WorkflowInstanceTableTestsBase
    {
        public WorkflowInstanceTableTests() : base(new ConfigurationTablesMemory(), new RuntimeTablesMemory()) { }
    }

    public class ActivityInstanceTableTests : ActivityInstanceTableTestsBase
    {
        public ActivityInstanceTableTests() : base(new ConfigurationTablesMemory(), new RuntimeTablesMemory()) { }
    }
}
