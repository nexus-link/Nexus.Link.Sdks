using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
{
    public class WorkflowPersistenceTest
    {

        private readonly WorkflowCache _workflowCache;
        private readonly WorkflowMgmtCapabilityMock _workflowMgmtCapability;
        private readonly IWorkflowImplementationBase _workflowImplementation;

        public WorkflowPersistenceTest()
        {
            var configurationTables = new ConfigurationTablesMemory();
            var runtimeTables = new RuntimeTablesMemory();

            var asyncRequestMgmtCapabilityMock = new Mock<IAsyncRequestMgmtCapability>();
            var asyncRequestClientMock = new Mock<IAsyncRequestClient>();
            _workflowMgmtCapability = new WorkflowMgmtCapabilityMock(configurationTables, runtimeTables, asyncRequestMgmtCapabilityMock.Object);
            _workflowImplementation = new TestWorkflowImplementation(_workflowMgmtCapability, asyncRequestClientMock.Object);
            var workflowInfo = new WorkflowInformation(_workflowImplementation);
            FulcrumAssert.IsValidated(workflowInfo);
            _workflowCache = new WorkflowCache(workflowInfo);
        }

        //[Fact]
        public async Task TODO()
        {
            var workflowServiceMock = new Mock<IWorkflowSummaryService>();
            _workflowMgmtCapability.Workflow = workflowServiceMock.Object;
            workflowServiceMock
                .Setup(x => x.GetSummaryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new FulcrumNotFoundException());

            await _workflowCache.SaveAsync(default);
        }
    }

    public class WorkflowMgmtCapabilityMock : WorkflowMgmtCapability
    {
        public WorkflowMgmtCapabilityMock(IConfigurationTables configurationTables, IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability)
            : base(configurationTables, runtimeTables, requestMgmtCapability)
        {
        }

        public new IWorkflowSummaryService Workflow { get; set; }
    }
}
