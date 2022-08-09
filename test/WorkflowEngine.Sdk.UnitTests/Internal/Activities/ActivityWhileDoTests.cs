using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivityWhileDoTests : ActivityTestsBase
{
    public ActivityWhileDoTests() : base(nameof(ActivityWhileDoTests))
    {
    }

    #region No return value
    [Fact]
    public async Task Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivityWhileDo(_activityInformationMock, (_, _) => Task.FromResult(true));
        activity.Do((_, _) => Task.CompletedTask);

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock
            .Verify(ae => ae.ExecuteWithoutReturnValueAsync(activity.WhileDoAsync, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task WhileDo_Given_IterationLength_Gives_CorrectNumberOfIteration(int expectedIterations)
    {
        // Arrange
        var logicExecutorMock = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutorMock;
        var activity = new ActivityWhileDo(_activityInformationMock, a => a.LoopIteration < expectedIterations);
        activity.Do(_ => {});

        // Act
        await activity.WhileDoAsync();

        // Assert
        logicExecutorMock.ExecuteWithReturnValueCounter.ShouldContainKey("While");
        logicExecutorMock.ExecuteWithReturnValueCounter["While"].ShouldBe(expectedIterations);
        logicExecutorMock.ExecuteWithReturnValueCounter.Count.ShouldBe(1);
        for (int i = 0; i < expectedIterations; i++)
        {
            var loopIteration = i + 1;
            var key = $"Do{loopIteration}";
            logicExecutorMock.ExecuteWithoutReturnValueCounter.ShouldContainKey(key);
            logicExecutorMock.ExecuteWithoutReturnValueCounter[key].ShouldBe(1);
        }
        logicExecutorMock.ExecuteWithoutReturnValueCounter.Count.ShouldBe(expectedIterations);
    }
    #endregion

    #region Return value
    [Fact]
    public async Task RV_Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivityWhileDo<int>(_activityInformationMock, null, (_, _) => Task.FromResult(true));
        activity.Do((_, _) => Task.FromResult(1));

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock
            .Verify(ae => ae.ExecuteWithReturnValueAsync(activity.WhileDoAsync, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task RV_WhileDo_Given_IterationLength_Gives_CorrectNumberOfIteration(int expectedIterations)
    {
        // Arrange
        var logicExecutorMock = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutorMock;
        var activity = new ActivityWhileDo<int>(_activityInformationMock, null, a => a.LoopIteration < expectedIterations);
        activity.Do(_ => 1);

        // Act
        await activity.WhileDoAsync();

        // Assert
        logicExecutorMock.ExecuteWithReturnValueCounter.ShouldContainKey("While");
        logicExecutorMock.ExecuteWithReturnValueCounter["While"].ShouldBe(expectedIterations);
        for (int i = 0; i < expectedIterations; i++)
        {
            var loopIteration = i + 1;
            var key = $"Do{loopIteration}";
            logicExecutorMock.ExecuteWithReturnValueCounter.ShouldContainKey(key);
            logicExecutorMock.ExecuteWithReturnValueCounter[key].ShouldBe(1);
        }
        logicExecutorMock.ExecuteWithReturnValueCounter.Count.ShouldBe(expectedIterations+1);
    }
    #endregion
    private Task<int> DefaultMethod(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}