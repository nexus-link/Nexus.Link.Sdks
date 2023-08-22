using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivityIfTests : ActivityTestsBase
{
    public ActivityIfTests() : base(nameof(ActivityIfTests))
    {
    }

    #region No return value
    [Fact]
    public async Task Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivityIf(_activityInformationMock, (_, _) => Task.FromResult(true));

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(activity.IfThenElseAsync, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IfThenElse_Given_Condition_Gives_CallsThenOrElse(bool condition)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityIf(_activityInformationMock, (_, _) => Task.FromResult(condition));
        activity.Then((_, _) => Task.CompletedTask);
        activity.Else((_, _) => Task.CompletedTask);

        // Act
        await activity.IfThenElseAsync();

        // Assert
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("If");
        logicExecutor.ExecuteWithReturnValueCounter["If"].ShouldBe(1);
        if (condition)
        {
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Then");
            logicExecutor.ExecuteWithoutReturnValueCounter["Then"].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Else");
        }
        else
        {
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Else");
            logicExecutor.ExecuteWithoutReturnValueCounter["Else"].ShouldBe(1);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IfThenElse_Given_NoThen_Gives_CallsElseButNotThen(bool condition)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityIf(_activityInformationMock, (_, _) => Task.FromResult(condition));
        activity.Else((_, _) => Task.CompletedTask);

        // Act
        await activity.IfThenElseAsync();

        // Assert
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("If");
        logicExecutor.ExecuteWithReturnValueCounter["If"].ShouldBe(1);
        logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Then");
        if (condition)
        {
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Else");
        }
        else
        {
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Else");
            logicExecutor.ExecuteWithoutReturnValueCounter["Else"].ShouldBe(1);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IfThenElse_Given_NoElse_Gives_CallsThenButNotElse(bool condition)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityIf(_activityInformationMock, (_, _) => Task.FromResult(condition));
        activity.Then((_, _) => Task.CompletedTask);

        // Act
        await activity.IfThenElseAsync();

        // Assert
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("If");
        logicExecutor.ExecuteWithReturnValueCounter["If"].ShouldBe(1);
        logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Else");
        if (condition)
        {
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Then");
            logicExecutor.ExecuteWithoutReturnValueCounter["Then"].ShouldBe(1);
        }
        else
        {
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Then");
        }
    }
    #endregion

    #region Return value
    [Fact]
    public async Task RV_Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivityIf<int>(_activityInformationMock, null, (_, _) => Task.FromResult(true));
        activity.Then((_, _) => Task.FromResult(1));
        activity.Else((_, _) => Task.FromResult(2));

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock.Verify(e => e.ExecuteWithReturnValueAsync(activity.IfThenElseAsync, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RV_Execute_Given_NoThen_Gives_Throws()
    {
        // Arrange
        var activity = new ActivityIf<int>(_activityInformationMock, null, (_, _) => Task.FromResult(true));
        activity.Else((_, _) => Task.FromResult(1));

        // Act && Assert
        // TODO: Why doesn't the following work?
        // await activity.ExecuteAsync()
        //    .ShouldThrowAsync<FulcrumContractException>();
        await Should.ThrowAsync<FulcrumContractException>(() => activity.ExecuteAsync());
    }

    [Fact]
    public async Task RV_Execute_Given_NoElse_Gives_Throws()
    {
        // Arrange
        var activity = new ActivityIf<int>(_activityInformationMock, null, (_, _) => Task.FromResult(true));
        activity.Then((_, _) => Task.FromResult(1));

        // Act & Assert
        // TODO: Why doesn't the following work?
        //var exception = await activity.ExecuteAsync()
        //    .ShouldThrowAsync<Exception>();
        await Should.ThrowAsync<FulcrumContractException>(() => activity.ExecuteAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RV_IfThenElse_Given_Condition_Gives_CallsThenOrElse(bool condition)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityIf<int>(_activityInformationMock, null, (_, _) => Task.FromResult(condition));
        activity.Then((_, _) => Task.FromResult(1));
        activity.Else((_, _) => Task.FromResult(2));

        // Act
        await activity.IfThenElseAsync();

        // Assert
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("If");
        logicExecutor.ExecuteWithReturnValueCounter["If"].ShouldBe(1);
        if (condition)
        {
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Then");
            logicExecutor.ExecuteWithReturnValueCounter["Then"].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Else");
        }
        else
        {
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Else");
            logicExecutor.ExecuteWithReturnValueCounter["Else"].ShouldBe(1);
        }
    }

    #endregion
}