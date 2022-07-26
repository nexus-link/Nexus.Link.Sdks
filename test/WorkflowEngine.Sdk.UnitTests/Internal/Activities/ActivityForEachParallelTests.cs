using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivityForEachParallelTests : ActivityTestsBase
{
    public ActivityForEachParallelTests() : base(nameof(ActivityForEachParallelTests))
    {
    }

    #region No return value

    [Fact]
    public async Task Execute_Given_ManyItems_Gives_1Call()
    {
        // Arrange
        var values = new List<int> { 3, 2, 1 };
        var activity = new ActivityForEachParallel<int>(_activityInformationMock, values, (_, _, _) => Task.CompletedTask);

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock
            .Verify(ae => ae.ExecuteWithoutReturnValueAsync(activity.ForEachParallelAsync, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ForEachParallel_Given_ThreeItems_Gives3Calls()
    {
        // Arrange
        var expectedLoopIteration = new List<int> { 1, 2, 3 };
        var activity = new ActivityForEachParallel<int>(_activityInformationMock, expectedLoopIteration,
            (_, _, _) => Task.CompletedTask);

        // Act
        await activity.ForEachParallelAsync();

        // Assert
        _logicExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.Is<string>(s => s.StartsWith("Item")), It.IsAny<CancellationToken>()), Times.Exactly(expectedLoopIteration.Count));
    }

    [Fact]
    public async Task ForEachParallel_Given_ItemsInOrder_CorrectLoopOrder()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var actualLoopIterations = new ConcurrentDictionary<int, int>();
        var expectedLoopIteration = new List<int> { 1, 2, 3 };
        var activity = new ActivityForEachParallel<int>(_activityInformationMock, expectedLoopIteration, async (i, a, ct) =>
        {
            await TaskHelper.RandomDelayAsync(TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(5), ct);
            actualLoopIterations.TryAdd(i, a.LoopIteration);
        });

        // Act
        await activity.ForEachParallelAsync();

        // Assert
        foreach (var (i, actualLoopIteration) in actualLoopIterations)
        {
            actualLoopIteration.ShouldBe(i);
        }
    }

    [Fact]
    public async Task ForEachParallel_NoResult_Given_Summation_Gives_CorrectSum()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var actualValue = 0;
        var lockObject = new object();
        var values = new List<int> { 1, 2, 3, 4, 5, 99 };
        var activity = new ActivityForEachParallel<int>(_activityInformationMock, values,
            (i, a, _) =>
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
    #endregion

    #region Result value

    [Fact]
    public async Task RV_Execute_Given_ManyItems_Gives_1Call()
    {
        // Arrange
        var values = new List<int> { 3, 2, 1, 0 };
        var activity = new ActivityForEachParallel<string, int>(_activityInformationMock, values, i => i.ToString(), (_, _, _) => Task.FromResult("a"));

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock.Verify(
            ae => ae.ExecuteWithReturnValueAsync(
                activity.ForEachParallelAsync,
                It.IsAny<ActivityDefaultValueMethodAsync<IDictionary<string, string>>>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RV_ForEachParallel_Given_ThreeItems_Gives3Calls()
    {
        // Arrange
        var expectedLoopIteration = new List<int> { 1, 2, 3 };
        var activity = new ActivityForEachParallel<int, int>(_activityInformationMock, expectedLoopIteration, item => item.ToString(), (_, _, _) => Task.FromResult(1));

        // Act
        await activity.ForEachParallelAsync();

        // Assert
        _logicExecutorMock.Verify(e => e.ExecuteWithReturnValueAsync(It.IsAny<InternalActivityMethodAsync<int>>(), It.Is<string>(s => s.StartsWith("Item")), It.IsAny<CancellationToken>()), Times.Exactly(expectedLoopIteration.Count));
    }

    [Fact]
    public async Task RV_ForEachParallel_Given_PartNumbers_Gives_CorrectResult()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
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
        result.Values.ShouldBe(new[] { "1", "2", "3", "4" });
    }



    #endregion
}