using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Moq;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
{
    public class ActivityActionTests
    {
        private readonly WorkflowInformation _workflowInformation;
        private readonly Mock<IAsyncRequestClient> _asyncRequestClientMock;

        public ActivityActionTests()
        {
            var configurationTables = new ConfigurationTablesMemory();
            var runtimeTables = new RuntimeTablesMemory();
            var workflowCapability = new WorkflowCapability(configurationTables, runtimeTables);
            _workflowInformation = new WorkflowInformation(workflowCapability, new MethodHandler("Workflow"));
            _asyncRequestClientMock = new Mock<IAsyncRequestClient>();
        }

        [Fact]
        public async Task Execute_Given_InternalMethod_Gives_Result()
        {
            // Arrange
            var activityInformation = new ActivityInformation(_workflowInformation, new MethodHandler("Activity"), 1,
                WorkflowActivityTypeEnum.Action, null, null);
            var activity = new ActivityAction<int>(activityInformation, _asyncRequestClientMock.Object, null, () => Task.FromResult(1));
            const int expectedValue = 10;

            // Act
            var result = await activity.ExecuteAsync((action, t) => Task.FromResult(expectedValue));

            // Assert
            Assert.Equal(expectedValue, result);
        }
    }
}
