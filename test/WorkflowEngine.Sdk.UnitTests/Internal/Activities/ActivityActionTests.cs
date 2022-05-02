using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Shouldly;
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
        public async Task Action_Given_NoReturnValue_Gives_Executed()
        {
            // Arrange
            const int expectedValue = 10;
            var actualValue = 0;
            var activity = new ActivityAction(_activityInformationMock, (a, ct) =>
            {
                actualValue = expectedValue;
                return Task.CompletedTask;
            });

            // Act
            await activity.ActionAsync();

            // Assert
            actualValue.ShouldBe(expectedValue);
        }

        [Fact]
        public async Task Action_Given_ReturnValue_Gives_ExpectedResult()
        {
            // Arrange
            const int expectedResult = 10;
            var activity = new ActivityAction<int>(_activityInformationMock, null, (a, ct) => Task.FromResult(expectedResult));

            // Act
            var result = await activity.ActionAsync();

            // Assert
            result.ShouldBe(expectedResult);
        }

        [Fact]
        public async Task Execute_Given_ReturnValue_Gives_Call()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityInformationMock, DefaultMethod, (a, ct) => Task.FromResult(10));

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(
                ae => ae.ExecuteWithReturnValueAsync<int>(It.IsAny<InternalActivityMethodAsync<int>>(), DefaultMethod, It.IsAny<CancellationToken>()), Times.Once);
        }

        private Task<int> DefaultMethod(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task Execute_Given_NoReturnValue_Gives_Call()
        {
            // Arrange
            var activity = new ActivityAction(_activityInformationMock, (a, ct) => Task.CompletedTask);

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock
                .Verify(ae => ae.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

