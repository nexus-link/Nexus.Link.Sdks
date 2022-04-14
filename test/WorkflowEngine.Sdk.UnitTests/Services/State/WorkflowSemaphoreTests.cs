using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Capabilities.WorkflowState.UnitTests.Services;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;

namespace WorkflowEngine.Sdk.UnitTests.Services.State
{
    public class WorkflowSemaphoreTests : WorkflowSemaphoreTestBase
    {
        private static readonly Mock<IRuntimeTables> RuntimeTablesMock = new Mock<IRuntimeTables>();

        public WorkflowSemaphoreTests()
        : base(new WorkflowSemaphoreService(new AsyncRequestMgmtMock(), RuntimeTablesMock.Object), new TestVerifier(RuntimeTablesMock.Object))
        {
            var runtimeTables = new RuntimeTablesMemory();
            RuntimeTablesMock.SetupGet(tables => tables.WorkflowSemaphore).Returns(runtimeTables.WorkflowSemaphore);
            RuntimeTablesMock.SetupGet(tables => tables.WorkflowSemaphoreQueue).Returns(runtimeTables.WorkflowSemaphoreQueue);
        }
    }

    public class TestVerifier : ITestVerify
    {
        private readonly IRuntimeTables _runtimeTables;

        public TestVerifier(IRuntimeTables runtimeTables)
        {
            _runtimeTables = runtimeTables;
        }

        /// <inheritdoc />
        public async Task<ICollection<string>> GetRaisedWorkflowInstanceIdsAsync()
        {
            var semaphores = await _runtimeTables.WorkflowSemaphoreQueue.SearchAsync(
                new SearchDetails<WorkflowSemaphoreQueueRecord>(
                    new
                    {
                        Raised = true
                    }), 0, 10);
            return semaphores.Data.Select(record => record.WorkflowInstanceId.ToGuidString()).ToArray();
        }
    }
}
