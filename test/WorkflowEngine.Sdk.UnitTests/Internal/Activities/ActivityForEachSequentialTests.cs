using System;
using System.Collections.Generic;
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
    public class ActivityForEachSequentialTests
    {
        private readonly Mock<IActivityExecutor> _activityExecutorMock;
        private readonly ActivityInformationMock _activityInformationMock;

        public ActivityForEachSequentialTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityForEachSequentialTests));
            _activityExecutorMock = new Mock<IActivityExecutor>();
            var workflowInformationMock = new WorkflowInformationMock(_activityExecutorMock.Object);
            _activityInformationMock = new ActivityInformationMock(workflowInformationMock);
        }

        [Fact]
        public async Task ForEachSequential_NoResult_Given_Summation_Gives_CorrectSum()
        {
            // Arrange
            var actualValue = 0;
            var lockObject = new object();
            var values = new List<int> { 1, 2, 3, 4, 5, 99 };
            var activity = new ActivityForEachSequential<int>(_activityInformationMock, values,
                (i, a, ct) =>
            {
                lock (lockObject)
                {
                    if (i == a.LoopIteration) actualValue += i;
                }
                return Task.CompletedTask;
            });

            // Act
            await activity.ForEachSequentialAsync();

            // Assert
            actualValue.ShouldBe(15);
        }

        [Fact]
        public async Task ForEachSequential_Result_Given_PartNumbers_Gives_CorrectResult()
        {
            // Arrange
            var values = new List<int> { 3, 2, 1, 99 };
            var activity = new ActivityForEachSequential<string, int>(_activityInformationMock, GetDefaultValueAsync, values,
                async (i, a, ct) =>
            {
                await Task.Delay(1, ct);
                return a.LoopIteration + ": " + i;
            });

            // Act
            var result = await activity.ForEachSequentialAsync();

            // Assert
            result.Count.ShouldBe(4);
            result.ShouldBe(new []{"1: 3", "2: 2", "3: 1", "4: 99"});
        }

        private Task<string> GetDefaultValueAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task Execute_Result_Given_ManyItems_Gives_1Call()
        {
            // Arrange
            var values = new List<int> { 3, 2, 1, 0 };
            var activity = new ActivityForEachSequential<string, int>(_activityInformationMock, GetDefaultValueAsync, values, (_,_,_) => Task.FromResult("a"));

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(
                ae => ae.ExecuteWithReturnValueAsync<IList<string>>(
                    It.IsAny<InternalActivityMethodAsync<IList<string>>>(),
                    It.IsAny<ActivityDefaultValueMethodAsync<IList<string>>>(), 
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Execute_NoResult_Given_ManyItems_Gives_1Call()
        {
            // Arrange
            var values = new List<int> { 3, 2, 1 };
            var activity = new ActivityForEachSequential<int>(_activityInformationMock, values, (_, _, _) => Task.CompletedTask);

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock
                .Verify(ae => ae.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

