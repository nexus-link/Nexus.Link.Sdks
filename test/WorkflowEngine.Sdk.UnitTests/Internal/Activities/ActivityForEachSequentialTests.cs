using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities
{
    public class ActivityForEachSequentialTests : ActivityTestsBase
    {
        public ActivityForEachSequentialTests() :base(nameof(ActivityForEachSequentialTests))
        {
        }

        #region No return value
        [Fact]
        public async Task Execute_Given_ManyItems_Gives_1Call()
        {
            // Arrange
            var values = new List<int> { 3, 2, 1 };
            var activity = new ActivityForEachSequential<int>(_activityInformationMock, values, (_, _, _) => Task.CompletedTask);

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock
                .Verify(ae => ae.ExecuteWithoutReturnValueAsync(activity.ForEachSequentialAsync, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ForEachSequential_Given_ManyItems_Gives_NumberOfCalls()
        {
            // Arrange
            var values = new List<int> { 1, 2, 3, 4, 5, 99 };
            var activity = new ActivityForEachSequential<int>(_activityInformationMock, values, (_, _, _) => Task.CompletedTask);

            // Act
            await activity.ForEachSequentialAsync();

            // Assert
            _logicExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.Is<string>(s => s.StartsWith("Item")), It.IsAny<CancellationToken>()), Times.Exactly(values.Count));
        }

        [Fact]
        public async Task ForEachSequential_Given_Summation_Gives_CorrectSum()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            var actualValue = 0;
            var lockObject = new object();
            var values = new List<int> { 1, 2, 3, 4, 5, 99 };
            var activity = new ActivityForEachSequential<int>(_activityInformationMock, values,
                (i, a, _) =>
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
        #endregion

        #region Return value
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
                ae => ae.ExecuteWithReturnValueAsync(
                    activity.ForEachSequentialAsync,
                    It.IsAny<ActivityDefaultValueMethodAsync<IList<string>>>(), 
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RV_ForEachSequential_Given_ManyItems_Gives_NumberOfCalls()
        {
            // Arrange
            var values = new List<int> { 1, 2, 3, 4, 5, 99 };
            var activity = new ActivityForEachSequential<int,int>(_activityInformationMock, null, values, (_,_, _) => Task.FromResult(1));

            // Act
            await activity.ForEachSequentialAsync();

            // Assert
            _logicExecutorMock.Verify(e => e.ExecuteWithReturnValueAsync(It.IsAny < InternalActivityMethodAsync<int>>(), It.Is<string>(s => s.StartsWith("Item")), It.IsAny<CancellationToken>()), Times.Exactly(values.Count));
        }


        [Fact]
        public async Task ForEachSequential_Result_Given_PartNumbers_Gives_CorrectResult()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
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
            result.ShouldBe(new[] { "1: 3", "2: 2", "3: 1", "4: 99" });
        }

        #endregion


        private Task<string> GetDefaultValueAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

