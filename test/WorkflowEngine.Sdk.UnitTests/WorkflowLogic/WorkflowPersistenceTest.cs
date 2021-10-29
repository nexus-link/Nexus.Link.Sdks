using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
{
    public class WorkflowPersistenceTest
    {

        private readonly WorkflowPersistence _persistence;
        private readonly WorkflowCapabilityMock _workflowCapability;

        public WorkflowPersistenceTest()
        {
            var configurationTables = new ConfigurationTablesMemory();
            var runtimeTables = new RuntimeTablesMemory();

            var asyncRequestMgmtCapabilityMock = new Mock<IAsyncRequestMgmtCapability>();
            _workflowCapability = new WorkflowCapabilityMock(configurationTables, runtimeTables, asyncRequestMgmtCapabilityMock.Object);

            _persistence = new WorkflowPersistence(_workflowCapability, new MethodHandler("Form title"));
        }

        [Fact]
        public async Task TODO()
        {
            var workflowServiceMock = new Mock<IWorkflowSummaryService>();
            _workflowCapability.Workflow = workflowServiceMock.Object;
            workflowServiceMock
                .Setup(x => x.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new FulcrumNotFoundException());

            await _persistence.PersistAsync(default);
        }
    }

    public class WorkflowCapabilityMock : WorkflowCapability
    {
        public WorkflowCapabilityMock(IConfigurationTables configurationTables, IRuntimeTables runtimeTables, IAsyncRequestMgmtCapability requestMgmtCapability) : base(configurationTables, runtimeTables, requestMgmtCapability)
        {
        }

        public new IWorkflowSummaryService Workflow { get; set; }
    }
}
