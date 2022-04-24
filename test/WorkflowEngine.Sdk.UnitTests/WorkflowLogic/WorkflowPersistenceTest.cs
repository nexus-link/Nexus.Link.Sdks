using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
{
    public class WorkflowPersistenceTest
    {
        public WorkflowPersistenceTest()
        {
            var configurationTables = new ConfigurationTablesMemory();
            var runtimeTables = new RuntimeTablesMemory();

            var asyncRequestMgmtCapabilityMock = new Mock<IAsyncRequestMgmtCapability>();
            var workflowCapabilities = new WorkflowCapabilities(configurationTables, runtimeTables, asyncRequestMgmtCapabilityMock.Object);
            var workflowImplementation = new TestWorkflowImplementation(workflowCapabilities);
        }
    }

    internal class WorkflowConfigurationCapabilityMock : WorkflowConfigurationCapability
    {
        public WorkflowConfigurationCapabilityMock(IConfigurationTables configurationTables)
            : base(configurationTables)
        {
        }
    }

    internal class WorkflowStateCapabilityMock : WorkflowStateCapability
    {
        public WorkflowStateCapabilityMock(IConfigurationTables configurationTables, IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability)
            : base(configurationTables, runtimeTables, requestMgmtCapability)
        {
        }
    }
}
