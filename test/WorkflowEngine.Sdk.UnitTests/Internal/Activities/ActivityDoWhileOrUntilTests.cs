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
    public class ActivityDoWhileOrUntilTests
    {
        private readonly Mock<IActivityExecutor> _activityExecutorMock;
        private readonly ActivityInformationMock _activityInformationMock;

        public ActivityDoWhileOrUntilTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityDoWhileOrUntilTests));
            _activityExecutorMock = new Mock<IActivityExecutor>();
            var workflowInformationMock = new WorkflowInformationMock(_activityExecutorMock.Object);
            _activityInformationMock = new ActivityInformationMock(workflowInformationMock);
        }

        [Theory]
        [InlineData(false, false, 1)]
        [InlineData(false, true, 1)]
        [InlineData(true, false, 1)]
        [InlineData(true, true, 1)]
        [InlineData(false, false, 2)]
        [InlineData(false, true, 2)]
        [InlineData(true, false, 2)]
        [InlineData(true, true, 2)]
        [InlineData(false, false, 3)]
        [InlineData(false, true, 3)]
        [InlineData(true, false, 3)]
        [InlineData(true, true, 3)]
        public async Task GetWhileCondition_(bool isWhileCondition, bool conditionValue, int style)
        {
            // Arrange
            var expectedWhileCondition = isWhileCondition ? conditionValue : !conditionValue;
            var activity = new ActivityDoWhileOrUntil(_activityInformationMock, (a, ct) => Task.CompletedTask);
            switch (style)
            {
                case 1:
                    ActivityConditionMethodAsync conditionMethodAsync = (a, ct) => Task.FromResult(conditionValue);
                    if (isWhileCondition) activity.While(conditionMethodAsync);
                    else activity.Until(conditionMethodAsync);
                    break;
                case 2:
                    ActivityConditionMethod conditionMethod = a => conditionValue;
                    if (isWhileCondition) activity.While(conditionMethod);
                    else activity.Until(conditionMethod);
                    break;
                case 3:
                    if (isWhileCondition) activity.While(conditionValue);
                    else activity.Until(conditionValue);
                    break;
            }

            // Act
            var actualWhileCondition = await activity.GetWhileConditionAsync();

            // Assert
            actualWhileCondition.ShouldBe(expectedWhileCondition);
        }

        [Fact]
        public async Task DoWhileOrUntil_Given_Iteration_Gives_Success()
        {
            // Arrange
            var actualValue = 0;
            const int expectedValue = 5;
            var activity = new ActivityDoWhileOrUntil(_activityInformationMock, (a, ct) =>
            {
                actualValue++;
                return Task.CompletedTask;
            });
            activity.While(a => actualValue < expectedValue);

            // Act
            await activity.DoWhileOrUntilAsync();

            // Assert
            actualValue.ShouldBe(expectedValue);
        }

        [Fact]
        public async Task DoWhileOrUntil_Given_ReturnValue_Gives_ExpectedResult()
        {
            // Arrange
            const int expectedResult = 10;
            var activity = new ActivityDoWhileOrUntil<int>(_activityInformationMock, null, (a, ct) => Task.FromResult(expectedResult));
            activity.Until(true);

            // Act
            var result = await activity.DoWhileOrUntilAsync();

            // Assert
            result.ShouldBe(expectedResult);
        }

        [Fact]
        public async Task Execute_NoReturn_Given_OneLoop_Gives_OnCall()
        {
            // Arrange
            var activity = new ActivityDoWhileOrUntil(_activityInformationMock, (a, ct) => Task.CompletedTask);
            activity.Until(true);

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock
                .Verify(ae => ae.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ReturnValue_Given_OneLoop_Gives_Call()
        {
            // Arrange
            var activity = new ActivityDoWhileOrUntil<int>(_activityInformationMock, DefaultMethod, (a, ct) => Task.FromResult(10));
            activity.Until(true);

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
    }
}

