using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.ConfigurationTablesTests;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;

namespace WorkflowEngine.Sdk.Persistence.Memory.UnitTests
{
    public class WorkflowFormTableTests : WorkflowFormTableTestsBase
    {
        public WorkflowFormTableTests() : base(new ConfigurationTablesMemory(), new RuntimeTablesMemory()) { }
    }
    public class WorkflowVersionTableTests : WorkflowVersionTableTestsBase
    {
        public WorkflowVersionTableTests() : base(new ConfigurationTablesMemory(), new RuntimeTablesMemory()) { }
    }
    public class ActivityFormTableTests : ActivityFormTableTestsBase
    {
        public ActivityFormTableTests() : base(new ConfigurationTablesMemory(), new RuntimeTablesMemory()) { }
    }
    public class ActivityVersionTableTests : ActivityVersionTableTestsBase
    {
        public ActivityVersionTableTests() : base(new ConfigurationTablesMemory(), new RuntimeTablesMemory()) { }
    }
}
