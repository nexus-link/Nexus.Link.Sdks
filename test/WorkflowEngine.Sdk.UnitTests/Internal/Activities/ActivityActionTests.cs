using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities
{
    public class ActivityActionTests
    {
        private readonly Mock<IActivityExecutor> _activityExecutorMock;
        private readonly ActivityInformationMock _activityInformationMock;

        public ActivityActionTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityActionTests));
            _activityExecutorMock = new Mock<IActivityExecutor>();
            var workflowInformationMock = new WorkflowInformationMock(_activityExecutorMock.Object);
            _activityInformationMock = new ActivityInformationMock(workflowInformationMock);
        }

        [Fact]
        public async Task Execute_Given_ReturnValue_Gives_Call()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityInformationMock, DefaultMethod);

            // Act
            await activity.ExecuteAsync((a, ct) => Task.FromResult(10));

            // Assert
            _activityExecutorMock.Verify(
                ae => ae.ExecuteWithReturnValueAsync<int>(It.IsAny<ActivityMethodAsync<int>>(), DefaultMethod, It.IsAny<CancellationToken>()), Times.Once);
        }

        private Task<int> DefaultMethod(CancellationToken arg)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task Execute_Given_NoReturnValue_Gives_Call()
        {
            // Arrange
            var activity = new ActivityAction(_activityInformationMock);
            // Act
            await activity.ExecuteAsync((a, ct) => Task.CompletedTask);

            // Assert
            _activityExecutorMock
                .Verify(ae => ae.ExecuteWithoutReturnValueAsync(It.IsAny<ActivityMethodAsync>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

