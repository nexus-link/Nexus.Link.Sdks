using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivityForEachSequentialTests : ActivityTestsBase
{
    public ActivityForEachSequentialTests() : base(nameof(ActivityForEachSequentialTests))
    {
    }

    #region No return value
    [Fact]
    public async Task Execute_Given_ManyItems_Gives_1Call()
    {
        // Arrange
        var values = new List<int> { 3, 2, 1 };
        var activity = new ActivityForEachSequential<int>(_activityInformationMock, values, null, (_, _, _) => Task.CompletedTask);

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
        var activity = new ActivityForEachSequential<int>(_activityInformationMock, values, null, (_, _, _) => Task.CompletedTask);

        // Act
        await activity.ForEachSequentialAsync();

        // Assert
        _logicExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.Is<string>(s => s.StartsWith("Item")), It.IsAny<CancellationToken>()), Times.Exactly(values.Count));
    }

    [Fact]
    public async Task ForEachSequentialAsync_Given_Summation_Gives_CorrectSum()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var actualValue = 0;
        var lockObject = new object();
        var values = new List<int> { 1, 2, 3, 4, 5, 99 };
        var activity = new ActivityForEachSequential<int>(_activityInformationMock, values, null,
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

    #region Legacy
#pragma warning disable CS0618


    [Fact]
    public async Task Legacy_Execute_Given_ManyItems_Gives_1Call()
    {
        // Arrange
        var values = new List<int> { 3, 2, 1 };
        var activity = new ActivityForEachSequential<int>(_activityInformationMock, values, null);

        // Act
        await activity.ExecuteAsync((_, _, _) => Task.CompletedTask);

        // Assert
        _activityExecutorMock
            .Verify(ae => ae.ExecuteWithoutReturnValueAsync(activity.ForEachSequentialAsync, It.IsAny<CancellationToken>()), Times.Once);
    }

#pragma warning restore CS0618
    #endregion
    #endregion

    #region Return value
    [Fact]
    public async Task RV_Execute_Result_Given_ManyItems_Gives_1Call()
    {
        // Arrange
        var values = new List<int> { 3, 2, 1, 0 };
        var activity = new ActivityForEachSequential<string, int>(_activityInformationMock, values, (_, _, _) => Task.FromResult("a"), null);

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
        var activity = new ActivityForEachSequential<int, int>(_activityInformationMock, values, (_, _, _) => Task.FromResult(1), null);

        // Act
        await activity.ForEachSequentialAsync();

        // Assert
        _logicExecutorMock.Verify(e => e.ExecuteWithReturnValueAsync(It.IsAny<InternalActivityMethodAsync<int>>(), It.Is<string>(s => s.StartsWith("Item")), It.IsAny<CancellationToken>()), Times.Exactly(values.Count));
    }


    [Fact]
    public async Task RV_ForEachSequential_Result_Given_PartNumbers_Gives_CorrectResult()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var values = new List<int> { 3, 2, 1, 99 };
        var activity = new ActivityForEachSequential<string, int>(_activityInformationMock, values,
            async (i, a, ct) =>
            {
                await Task.Delay(1, ct);
                return a.LoopIteration + ": " + i;
            }, null);

        // Act
        var result = await activity.ForEachSequentialAsync();

        // Assert
        result.Count.ShouldBe(4);
        result.ShouldBe(new[] { "1: 3", "2: 2", "3: 1", "4: 99" });
    }

    #region Legacy
#pragma warning disable CS0618
    [Fact]
    public async Task Legacy_Execute_Result_Given_ManyItems_Gives_1Call()
    {
        // Arrange
        var values = new List<int> { 3, 2, 1, 0 };
        var activity = new ActivityForEachSequential<string, int>(_activityInformationMock, values);

        // Act
        await activity.ExecuteAsync((_, _, _) => Task.FromResult("a"));

        // Assert
        _activityExecutorMock.Verify(
            ae => ae.ExecuteWithReturnValueAsync(
                activity.ForEachSequentialAsync,
                It.IsAny<ActivityDefaultValueMethodAsync<IList<string>>>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }
#pragma warning restore CS0618
    #endregion

    #endregion
}