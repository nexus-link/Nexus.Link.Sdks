using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Model;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
{
    public class WorkflowPersistenceTest
    {

        private readonly WorkflowCache _workflowCache;
        private readonly IWorkflowConfigurationCapability _workflowConfigurationCapability;
        private readonly IWorkflowStateCapability _workflowStateCapability;

        public WorkflowPersistenceTest()
        {
            var configurationTables = new ConfigurationTablesMemory();
            var runtimeTables = new RuntimeTablesMemory();

            var asyncRequestMgmtCapabilityMock = new Mock<IAsyncRequestMgmtCapability>();
            _workflowConfigurationCapability = new WorkflowConfigurationCapabilityMock(configurationTables);
            _workflowStateCapability = new WorkflowStateCapabilityMock(configurationTables, runtimeTables, asyncRequestMgmtCapabilityMock.Object);
            var workflowCapabilities = new WorkflowCapabilities(configurationTables, runtimeTables, asyncRequestMgmtCapabilityMock.Object);
            var workflowImplementation = new TestWorkflowImplementation(workflowCapabilities);
            var workflowInfo = new WorkflowInformation(workflowImplementation);
            FulcrumAssert.IsValidated(workflowInfo);
            _workflowCache = new WorkflowCache(workflowInfo);
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
