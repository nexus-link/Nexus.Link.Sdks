using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivitySwitchTests : ActivityTestsBase
{
    public ActivitySwitchTests() : base(nameof(ActivitySwitchTests))
    {
    }

    #region No return value
    [Fact]
    public async Task Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivitySwitch<int>(_activityInformationMock, (_, _) => Task.FromResult(1));

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(activity.SwitchAsync, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    public async Task Switch_Given_SwitchValue_Gives_CallsCaseOrDefault(int switchValue, bool expectDefaultExecuted)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var caseExecuted = new Dictionary<int, bool>();
        var defaultExecuted = false;
        caseExecuted[switchValue] = false;
        var activity = new ActivitySwitch<int>(_activityInformationMock, (_, _) => Task.FromResult(switchValue));
        activity.Case(1, (_, _) =>
        {
            caseExecuted[1] = true;
            return Task.CompletedTask;
        });
        activity.Case(2, (_, _) =>
        {
            caseExecuted[2] = true;
            return Task.CompletedTask;
        });
        activity.Default((_, _) =>
        {
            defaultExecuted = true;
            return Task.CompletedTask;
        });

        // Act
        await activity.SwitchAsync();

        // Assert
        foreach (var pair in caseExecuted)
        {
            if (switchValue.Equals(pair.Key) && !expectDefaultExecuted)
            {
                pair.Value.ShouldBe(true);
            }
            else
            {
                pair.Value.ShouldBe(false);
            }
        }
        defaultExecuted.ShouldBe(expectDefaultExecuted);

        // Assert
        logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(1);
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Switch");
        logicExecutor.ExecuteWithReturnValueCounter["Switch"].ShouldBe(1);
        logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(1);
        var caseValue = expectDefaultExecuted? "default" : switchValue.ToString();
        var key = $"Case {caseValue}";
        logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey(key);
        logicExecutor.ExecuteWithoutReturnValueCounter[key].ShouldBe(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task Switch_Given_NoDefault_Gives_CallsNone(int switchValue)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var caseExecuted = new Dictionary<int, bool>();
        caseExecuted[switchValue] = false;
        var activity = new ActivitySwitch<int>(_activityInformationMock, (_, _) => Task.FromResult(switchValue));
        activity.Case(1, (_, _) =>
        {
            caseExecuted[1] = true;
            return Task.CompletedTask;
        });
        activity.Case(2, (_, _) =>
        {
            caseExecuted[2] = true;
            return Task.CompletedTask;
        });

        // Act
        await activity.SwitchAsync();

        // Assert
        foreach (var pair in caseExecuted)
        {
            pair.Value.ShouldBe(false);
        }

        // Assert
        logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
        logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(1);
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Switch");
        logicExecutor.ExecuteWithReturnValueCounter["Switch"].ShouldBe(1);
    }
    #endregion

    #region Return value
    [Fact]
    public async Task RV_Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivitySwitch<int, int>(_activityInformationMock, null, (_, _) => Task.FromResult(1));

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock.Verify(e => e.ExecuteWithReturnValueAsync(activity.SwitchAsync, It.IsAny<ActivityDefaultValueMethodAsync<int>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    public async Task RV_Switch_Given_SwitchValue_Gives_CallsCaseOrDefault(int switchValue, bool expectDefaultExecuted)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var caseExecuted = new Dictionary<int, bool>();
        var defaultExecuted = false;
        caseExecuted[switchValue] = false;
        var activity = new ActivitySwitch<int, int>(_activityInformationMock, null, (_, _) => Task.FromResult(switchValue));
        activity.Case(1, (_, _) =>
        {
            caseExecuted[1] = true;
            return Task.FromResult(11);
        });
        activity.Case(2, (_, _) =>
        {
            caseExecuted[2] = true;
            return Task.FromResult(12);
        });
        activity.Default((_, _) =>
        {
            defaultExecuted = true;
            return Task.FromResult(99);
        });

        // Act
        var value = await activity.SwitchAsync();

        // Assert
        foreach (var pair in caseExecuted)
        {
            if (switchValue.Equals(pair.Key) && !expectDefaultExecuted)
            {
                pair.Value.ShouldBe(true);
                value.ShouldBe(pair.Key + 10);
            }
            else
            {
                pair.Value.ShouldBe(false);
            }
        }
        defaultExecuted.ShouldBe(expectDefaultExecuted);
        if (defaultExecuted) value.ShouldBe(99);

        // Assert
        logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
        logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(2);
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Switch");
        logicExecutor.ExecuteWithReturnValueCounter["Switch"].ShouldBe(1);
        var caseValue = expectDefaultExecuted ? "default" : switchValue.ToString();
        var key = $"Case {caseValue}";
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey(key);
        logicExecutor.ExecuteWithReturnValueCounter[key].ShouldBe(1);
    }

    [Fact]
    public async Task RV_Switch_Given_NoDefault_Gives_Throws()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivitySwitch<int, int>(_activityInformationMock, null, (_, _) => Task.FromResult(3));
        activity.Case(1, (_, _) => Task.FromResult(11));
        activity.Case(2, (_, _) => Task.FromResult(12));

        // Act & Assert
        // TODO: Why doesn't this work?
        //await activity.ExecuteAsync()
        //    .ShouldThrowAsync<FulcrumContractException>();
        try
        {
            await activity.SwitchAsync();
            FulcrumAssert.Fail(CodeLocation.AsString());
        }
        catch (ActivityFailedException)
        {
            // OK
        }
        catch (Exception)
        {
            FulcrumAssert.Fail(CodeLocation.AsString());
        }

        // Assert
        logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
        logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(1);
        logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Switch");
        logicExecutor.ExecuteWithReturnValueCounter["Switch"].ShouldBe(1);
    }
    #endregion
}