using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivityDoWhileOrUntilTests : ActivityTestsBase
{
    public ActivityDoWhileOrUntilTests() : base(nameof(ActivityDoWhileOrUntilTests))
    {
    }

    #region No return value
    [Fact]
    public async Task Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivityDoWhileOrUntil(_activityInformationMock, (_, _) => Task.CompletedTask);
        activity.Until(true);

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock
            .Verify(ae => ae.ExecuteWithoutReturnValueAsync(activity.DoWhileOrUntilAsync, It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task GetWhileCondition_Given_Style_PicksCorrect(bool isWhileCondition, bool conditionValue, int style)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var expectedWhileCondition = isWhileCondition ? conditionValue : !conditionValue;
        var activity = new ActivityDoWhileOrUntil(_activityInformationMock, (_, _) => Task.CompletedTask);
        switch (style)
        {
            case 1:
                Task<bool> ConditionMethodAsync(IActivityDoWhileOrUntil a, CancellationToken ct) => Task.FromResult(conditionValue);
                if (isWhileCondition) activity.While(ConditionMethodAsync);
                else activity.Until(ConditionMethodAsync);
                break;
            case 2:
                bool ConditionMethod(IActivityDoWhileOrUntil a) => conditionValue;
                if (isWhileCondition) activity.While(ConditionMethod);
                else activity.Until(ConditionMethod);
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

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task DoWhileOrUntil_Given_While_Gives_CorrectNumberOfIteration(int expectedIterations)
    {
        // Arrange
        var logicExecutorMock = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutorMock;
        var activity = new ActivityDoWhileOrUntil(_activityInformationMock, (_, _) => Task.CompletedTask);
        activity.While(a => a.LoopIteration < expectedIterations);

        // Act
        await activity.DoWhileOrUntilAsync();

        // Assert
        logicExecutorMock.ExecuteWithoutReturnValueCounter.ShouldContainKey("Do");
        logicExecutorMock.ExecuteWithoutReturnValueCounter["Do"].ShouldBe(expectedIterations);
        logicExecutorMock.ExecuteWithReturnValueCounter.ShouldContainKey("While");
        logicExecutorMock.ExecuteWithReturnValueCounter["While"].ShouldBe(expectedIterations);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task DoWhileOrUntil_Given_Until_Gives_CorrectNumberOfIteration(int expectedIterations)
    {
        // Arrange
        var logicExecutorMock = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutorMock;
        var activity = new ActivityDoWhileOrUntil(_activityInformationMock, (_, _) => Task.CompletedTask);
        activity.Until(a => a.LoopIteration >= expectedIterations);

        // Act
        await activity.DoWhileOrUntilAsync();

        // Assert
        logicExecutorMock.ExecuteWithoutReturnValueCounter.ShouldContainKey("Do");
        logicExecutorMock.ExecuteWithoutReturnValueCounter["Do"].ShouldBe(expectedIterations);
        logicExecutorMock.ExecuteWithReturnValueCounter.ShouldContainKey("Until");
        logicExecutorMock.ExecuteWithReturnValueCounter["Until"].ShouldBe(expectedIterations);
    }
    #endregion

    #region Return value
    [Fact]
    public async Task RV_Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivityDoWhileOrUntil<int>(_activityInformationMock, DefaultMethod, (_, _) => Task.FromResult(10));
        activity.Until(true);

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock.Verify(
            ae => ae.ExecuteWithReturnValueAsync(activity.DoWhileOrUntilAsync, DefaultMethod, It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task RV_GetWhileCondition_Given_Style_PicksCorrect(bool isWhileCondition, bool conditionValue, int style)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var expectedWhileCondition = isWhileCondition ? conditionValue : !conditionValue;
        var activity = new ActivityDoWhileOrUntil<int>(_activityInformationMock, null, (_, _) => Task.FromResult(1));
        switch (style)
        {
            case 1:
                Task<bool> ConditionMethodAsync(IActivityDoWhileOrUntil<int> a, CancellationToken ct) => Task.FromResult(conditionValue);
                if (isWhileCondition) activity.While(ConditionMethodAsync);
                else activity.Until(ConditionMethodAsync);
                break;
            case 2:
                bool ConditionMethod(IActivityDoWhileOrUntil<int> a) => conditionValue;
                if (isWhileCondition) activity.While(ConditionMethod);
                else activity.Until(ConditionMethod);
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

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task RV_DoWhileOrUntil_Given_While_Gives_CorrectNumberOfIteration(int expectedIterations)
    {
        // Arrange
        var logicExecutorMock = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutorMock;
        var activity = new ActivityDoWhileOrUntil<int>(_activityInformationMock, null, (_, _) => Task.FromResult(1));
        bool ActivityConditionMethod(IActivityDoWhileOrUntil<int> a) => a.LoopIteration < expectedIterations;
        activity.While(ActivityConditionMethod);

        // Act
        await activity.DoWhileOrUntilAsync();

        // Assert
        logicExecutorMock.ExecuteWithReturnValueCounter.ShouldContainKey("Do");
        logicExecutorMock.ExecuteWithReturnValueCounter["Do"].ShouldBe(expectedIterations);
        logicExecutorMock.ExecuteWithReturnValueCounter.ShouldContainKey("While");
        logicExecutorMock.ExecuteWithReturnValueCounter["While"].ShouldBe(expectedIterations);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task RV_DoWhileOrUntil_Given_Until_Gives_CorrectNumberOfIteration(int expectedIterations)
    {
        // Arrange
        var logicExecutorMock = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutorMock;
        var activity = new ActivityDoWhileOrUntil<int>(_activityInformationMock, null, (_, _) => Task.FromResult(1));
        activity.Until(a => a.LoopIteration >= expectedIterations);

        // Act
        await activity.DoWhileOrUntilAsync();

        // Assert
        logicExecutorMock.ExecuteWithReturnValueCounter.ShouldContainKey("Do");
        logicExecutorMock.ExecuteWithReturnValueCounter["Do"].ShouldBe(expectedIterations);
        logicExecutorMock.ExecuteWithReturnValueCounter.ShouldContainKey("Until");
        logicExecutorMock.ExecuteWithReturnValueCounter["Until"].ShouldBe(expectedIterations);
    }

    [Fact]
    public async Task RV_DoWhileOrUntil_Given_ImmediateReturn_Gives_ExpectedResult()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        const int expectedResult = 10;
        var activity = new ActivityDoWhileOrUntil<int>(_activityInformationMock, null, (_, _) => Task.FromResult(expectedResult));
        activity.Until(true);

        // Act
        var result = await activity.DoWhileOrUntilAsync();

        // Assert
        result.ShouldBe(expectedResult);
    }
    #endregion
    private Task<int> DefaultMethod(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}