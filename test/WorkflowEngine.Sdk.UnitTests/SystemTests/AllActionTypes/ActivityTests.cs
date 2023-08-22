using System;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.SystemTests.AllActionTypes.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.AllActionTypes;

public class ActivityTests : Base
{

    public ActivityTests() : base(nameof(ActivityTests))
    {
    }

    [Fact]
    public async Task Execute_GivenAllReturns_Gives_Completed()
    {
        // Arrange
        var parameterValue = 2;
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(AllActivityTypesContainer.ParameterNames.ParameterA, parameterValue);
        LogicMoq
            .SetupGet(l => l.IfValue)
            .Returns(true)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.SwitchValue)
            .Returns(1)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.WhileIncompleteName)
            .Returns("Incomplete")
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.DoUntilDoneName)
            .Returns("Done")
            .Verifiable();
        LogicMoq
            .Setup(l => l.DoUntilAsync(It.IsAny<IActivityDoWhileOrUntil>()))
            .Callback((IActivityDoWhileOrUntil a) => a.SetContext("Done", true))
            .Returns(Task.CompletedTask)
            .Verifiable();
        LogicMoq
            .Setup(l => l.WhileDoAsync(It.IsAny<IActivityWhileDo>()))
            .Callback((IActivityWhileDo a) => a.SetContext("Incomplete", false))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var result = await implementation.ExecuteAsync();

        // Assert
        LogicMoq.Verify();
        LogicMoq.Verify(l => l.ActionAsync(), Times.Once);
        LogicMoq.Verify(l => l.IfThenAsync(), Times.Once);
        LogicMoq.Verify(l => l.SwitchValue1Async(), Times.Once);
        LogicMoq.Verify(l => l.ForEachParallelAsync(1), Times.Once);
        LogicMoq.Verify(l => l.ForEachParallelAsync(2), Times.Once);
        LogicMoq.Verify(l => l.ForEachSequentialAsync(1), Times.Once);
        LogicMoq.Verify(l => l.ForEachSequentialAsync(2), Times.Once);
        LogicMoq.Verify(l => l.ActionWithThrottleAsync(), Times.Once);
        LogicMoq.Verify(l => l.ActionUnderLockAsync(), Times.Once);
        LogicMoq.Verify(l => l.ParallelJob1Async(), Times.Once);
        LogicMoq.Verify(l => l.ParallelJob2Async(), Times.Once);
        LogicMoq.VerifyNoOtherCalls();
        result.ShouldBe(parameterValue);
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Success.ToString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_TwoExecution(bool ifValue)
    {
        // Arrange
        var parameterValue = 3;
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(AllActivityTypesContainer.ParameterNames.ParameterA, parameterValue);

        // First run
        LogicMoq.
            Setup(l => l.ActionAsync())
            .ThrowsAsync(new RequestPostponedException());
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Second run
        LogicMoq.
            Setup(l => l.ActionAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.IfValue)
            .Returns(ifValue)
            .Verifiable();
        if (ifValue)
        {
            LogicMoq
                .Setup(l => l.IfThenAsync())
                .ThrowsAsync(new RequestPostponedException())
                .Verifiable();
        }
        else
        {
            LogicMoq
                .Setup(l => l.IfElseAsync())
                .ThrowsAsync(new RequestPostponedException())
                .Verifiable(); ;
        }

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        LogicMoq.Verify(l => l.ActionAsync(), Times.Exactly(2));

        LogicMoq.VerifyNoOtherCalls();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Waiting.ToString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_If(bool ifValue)
    {
        // Arrange
        var parameterValue = 3;
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(AllActivityTypesContainer.ParameterNames.ParameterA, parameterValue);

        LogicMoq
               .SetupGet(l => l.IfValue)
               .Returns(ifValue)
               .Verifiable();
        if (ifValue)
        {
            LogicMoq
                .Setup(l => l.IfThenAsync())
                .ThrowsAsync(new RequestPostponedException())
                .Verifiable();
        }
        else
        {
            LogicMoq
                .Setup(l => l.IfElseAsync())
                .ThrowsAsync(new RequestPostponedException())
                .Verifiable(); ;
        }

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        LogicMoq.Verify(l => l.ActionAsync(), Times.Once);

        LogicMoq.VerifyNoOtherCalls();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Waiting.ToString());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Execute_Switch(int switchValue)
    {
        // Arrange
        var parameterValue = 3;
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(AllActivityTypesContainer.ParameterNames.ParameterA, parameterValue);

        LogicMoq
            .SetupGet(l => l.IfValue)
            .Returns(true)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.SwitchValue)
            .Returns(switchValue)
            .Verifiable();
        switch (switchValue)
        {
            case 1:
                LogicMoq
                    .Setup(l => l.SwitchValue1Async())
                    .ThrowsAsync(new RequestPostponedException())
                    .Verifiable();
                break;
            case 2:
                LogicMoq
                .Setup(l => l.SwitchValue2Async())
                .ThrowsAsync(new RequestPostponedException())
                .Verifiable();
                break;
            default:
                throw new ArgumentException();
        }

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        LogicMoq.Verify(l => l.ActionAsync(), Times.Once);
        LogicMoq.Verify(l => l.IfThenAsync(), Times.Once);

        LogicMoq.VerifyNoOtherCalls();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Waiting.ToString());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task Execute_ForEachParallel(int numberOfItems)
    {
        // Arrange
        var parameterValue = numberOfItems;
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(AllActivityTypesContainer.ParameterNames.ParameterA, parameterValue);

        // First run
        LogicMoq
            .SetupGet(l => l.IfValue)
            .Returns(true)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.SwitchValue)
            .Returns(1)
            .Verifiable();
        for (var i = 0; i < numberOfItems; i++)
        {
            var item = i + 1;
            LogicMoq
                .Setup(l => l.ForEachParallelAsync(item))
                .ThrowsAsync(new RequestPostponedException())
                .Verifiable();
        }

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        LogicMoq.Verify(l => l.ActionAsync(), Times.Once);
        LogicMoq.Verify(l => l.IfThenAsync(), Times.Once);
        LogicMoq.Verify(l => l.SwitchValue1Async(), Times.Once);

        LogicMoq.VerifyNoOtherCalls();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Waiting.ToString());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task Execute_ForEachSequential(int numberOfItems)
    {
        // Arrange
        var parameterValue = numberOfItems;
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(AllActivityTypesContainer.ParameterNames.ParameterA, parameterValue);

        // First run
        LogicMoq
            .SetupGet(l => l.IfValue)
            .Returns(true)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.SwitchValue)
            .Returns(1)
            .Verifiable();
        LogicMoq
            .Setup(l => l.ForEachSequentialAsync(1))
            .ThrowsAsync(new RequestPostponedException())
            .Verifiable();

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        LogicMoq.Verify(l => l.ActionAsync(), Times.Once);
        LogicMoq.Verify(l => l.IfThenAsync(), Times.Once);
        LogicMoq.Verify(l => l.SwitchValue1Async(), Times.Once);
        for (var i = 0; i < numberOfItems; i++)
        {
            var item = i + 1;
            LogicMoq.Verify(l => l.ForEachParallelAsync(item), Times.Once);
        }
        LogicMoq.VerifyNoOtherCalls();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Waiting.ToString());
    }

    [Fact]
    public async Task Execute_Parallel()
    {
        // Arrange
        var parameterValue = 1;
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(AllActivityTypesContainer.ParameterNames.ParameterA, parameterValue);

        // First run
        LogicMoq
            .SetupGet(l => l.IfValue)
            .Returns(true)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.SwitchValue)
            .Returns(1)
            .Verifiable();
        LogicMoq
            .Setup(l => l.ParallelJob1Async())
            .ThrowsAsync(new RequestPostponedException())
            .Verifiable();
        LogicMoq
            .Setup(l => l.ParallelJob2Async())
            .ThrowsAsync(new RequestPostponedException())
            .Verifiable();

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        LogicMoq.Verify(l => l.ActionAsync(), Times.Once);
        LogicMoq.Verify(l => l.IfThenAsync(), Times.Once);
        LogicMoq.Verify(l => l.SwitchValue1Async(), Times.Once);
        LogicMoq.Verify(l => l.ForEachParallelAsync(1), Times.Once);
        LogicMoq.Verify(l => l.ForEachSequentialAsync(1), Times.Once);
        LogicMoq.Verify(l => l.ActionWithThrottleAsync(), Times.Once);
        LogicMoq.Verify(l => l.ActionUnderLockAsync(), Times.Once);
        LogicMoq.VerifyNoOtherCalls();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Waiting.ToString());
    }

    [Fact]
    public async Task Execute_DoUntil()
    {
        // Arrange
        var parameterValue = 1;
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(AllActivityTypesContainer.ParameterNames.ParameterA, parameterValue);

        // First run
        LogicMoq
            .SetupGet(l => l.IfValue)
            .Returns(true)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.SwitchValue)
            .Returns(1)
            .Verifiable();
        LogicMoq
            .Setup(l => l.DoUntilAsync(It.IsAny<IActivityDoWhileOrUntil>()))
            .ThrowsAsync(new RequestPostponedException())
            .Verifiable();

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        LogicMoq.Verify(l => l.ActionAsync(), Times.Once);
        LogicMoq.Verify(l => l.IfThenAsync(), Times.Once);
        LogicMoq.Verify(l => l.SwitchValue1Async(), Times.Once);
        LogicMoq.Verify(l => l.ForEachParallelAsync(1), Times.Once);
        LogicMoq.Verify(l => l.ForEachSequentialAsync(1), Times.Once);
        LogicMoq.Verify(l => l.ActionWithThrottleAsync(), Times.Once);
        LogicMoq.Verify(l => l.ActionUnderLockAsync(), Times.Once);
        LogicMoq.Verify(l => l.ParallelJob1Async(), Times.Once);
        LogicMoq.Verify(l => l.ParallelJob2Async(), Times.Once);
        LogicMoq.VerifyNoOtherCalls();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Waiting.ToString());
    }

    [Fact]
    public async Task Execute_WhileDo()
    {
        // Arrange
        var parameterValue = 1;
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(AllActivityTypesContainer.ParameterNames.ParameterA, parameterValue);
        LogicMoq
            .SetupGet(l => l.IfValue)
            .Returns(true)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.SwitchValue)
            .Returns(1)
            .Verifiable();
        LogicMoq
            .SetupGet(l => l.DoUntilDoneName)
            .Returns("Done")
            .Verifiable();
        LogicMoq
            .Setup(l => l.DoUntilAsync(It.IsAny<IActivityDoWhileOrUntil>()))
            .Callback((IActivityDoWhileOrUntil a) => a.SetContext("Done", true))
            .Returns(Task.CompletedTask)
            .Verifiable();
        LogicMoq
            .Setup(l => l.WhileDoAsync(It.IsAny<IActivityWhileDo>()))
            .ThrowsAsync(new RequestPostponedException())
            .Verifiable();

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        LogicMoq.Verify(l => l.ActionAsync(), Times.Once);
        LogicMoq.Verify(l => l.IfThenAsync(), Times.Once);
        LogicMoq.Verify(l => l.SwitchValue1Async(), Times.Once);
        LogicMoq.Verify(l => l.ForEachParallelAsync(1), Times.Once);
        LogicMoq.Verify(l => l.ForEachSequentialAsync(1), Times.Once);
        LogicMoq.Verify(l => l.ActionWithThrottleAsync(), Times.Once);
        LogicMoq.Verify(l => l.ActionUnderLockAsync(), Times.Once);
        LogicMoq.Verify(l => l.ParallelJob1Async(), Times.Once);
        LogicMoq.Verify(l => l.ParallelJob2Async(), Times.Once);
        LogicMoq.VerifyNoOtherCalls();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Waiting.ToString());
    }
}