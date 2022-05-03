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
    public class ActivityForEachParallelTests
    {
        private readonly Mock<IActivityExecutor> _activityExecutorMock;
        private readonly ActivityInformationMock _activityInformationMock;

        public ActivityForEachParallelTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityForEachParallelTests));
            _activityExecutorMock = new Mock<IActivityExecutor>();
            var workflowInformationMock = new WorkflowInformationMock(_activityExecutorMock.Object);
            _activityInformationMock = new ActivityInformationMock(workflowInformationMock);
        }

        [Fact]
        public async Task ForEachParallel_NoResult_Given_Summation_Gives_CorrectSum()
        {
            // Arrange
            var actualValue = 0;
            var lockObject = new object();
            var values = new List<int> { 1, 2, 3, 4, 5, 99 };
            var activity = new ActivityForEachParallel<int>(_activityInformationMock, values,
                (i, a, ct) =>
            {
                lock (lockObject)
                {
                    if (i == a.LoopIteration) actualValue += i;
                }
                return Task.CompletedTask;
            });

            // Act
            await activity.ForEachParallelAsync();

            // Assert
            actualValue.ShouldBe(15);
        }

        [Fact]
        public async Task ForEachParallel_Result_Given_PartNumbers_Gives_CorrectResult()
        {
            // Arrange
            var values = new List<int> { 3, 2, 1, 99 };
            var activity = new ActivityForEachParallel<string, int>(_activityInformationMock, values,
                i => i.ToString(),
                async (_, a, ct) =>
            {
                await Task.Delay(1, ct);
                return a.LoopIteration.ToString();
            });

            // Act
            var result = await activity.ForEachParallelAsync();

            // Assert
            result.Count.ShouldBe(4);
            result.Values.ShouldBe(new []{"1", "2", "3", "4"});
        }

        [Fact]
        public async Task Execute_Result_Given_ManyItems_Gives_1Call()
        {
            // Arrange
            var values = new List<int> { 3, 2, 1, 0 };
            var activity = new ActivityForEachParallel<string, int>(_activityInformationMock, values, i => i.ToString(), (_,_,_) => Task.FromResult("a"));

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(
                ae => ae.ExecuteWithReturnValueAsync(
                    It.IsAny<InternalActivityMethodAsync<IDictionary<string, string>>>(),
                    It.IsAny<ActivityDefaultValueMethodAsync<IDictionary<string, string>>>(), 
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Execute_NoResult_Given_ManyItems_Gives_1Call()
        {
            // Arrange
            var values = new List<int> { 3, 2, 1 };
            var activity = new ActivityForEachParallel<int>(_activityInformationMock, values, (_, _, _) => Task.CompletedTask);

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock
                .Verify(ae => ae.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

