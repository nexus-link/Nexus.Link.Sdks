using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivityActionTests : ActivityTestsBase
{
    public ActivityActionTests() :base(nameof(ActivityActionTests))
    {
    }

    #region No return value
    [Fact]
    public async Task Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivityAction(_activityInformationMock, (_, _) => Task.CompletedTask);

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(activity.ActionAsync, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InternalExecute_Given_Normal_Gives_LogicExecutorActivated()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction(_activityInformationMock, (_, _) => Task.CompletedTask);

        // Act
        await activity.ActionAsync();

        // Assert
        logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(0);
        logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(1);
        logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Action");
        logicExecutor.ExecuteWithoutReturnValueCounter["Action"].ShouldBe(1);
    }
    #endregion

    #region Return value
    [Fact]
    public async Task RV_Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => Task.FromResult(1));

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock.Verify(e => e.ExecuteWithReturnValueAsync(activity.ActionAsync, It.IsAny<ActivityDefaultValueMethodAsync<int>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RV_InternalExecute_Given_Normal_Gives_LogicExecutorActivated()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => Task.FromResult(1));

        // Act
        await activity.ActionAsync();

        // Assert
        logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(1);
        logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Action");
        logicExecutor.ExecuteWithReturnValueCounter["Action"].ShouldBe(1);
    }
    #endregion
}