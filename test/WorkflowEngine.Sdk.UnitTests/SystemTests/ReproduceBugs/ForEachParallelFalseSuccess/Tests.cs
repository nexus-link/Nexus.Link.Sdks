using System;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.SystemTests.AllActionTypes.Support;
using WorkflowEngine.Sdk.UnitTests.SystemTests.ReproduceBugs.ForEachParallelFalseSuccess.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.ReproduceBugs.ForEachParallelFalseSuccess;

public class Tests() : TestBase(nameof(Tests))
{
    [Fact]
    public async Task Execute_GivenOneWaiting_Gives_Waiting()
    {
        // Arrange
        var implementation = await WorkflowContainer.SelectImplementationAsync<int>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        LogicMoq
            .Setup(l => l.Action1Async(It.IsAny<IActivityAction>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        LogicMoq
            .Setup(l => l.Action2Async(It.IsAny<IActivityAction>()))
            .ThrowsAsync(new ActivityPostponedException(TimeSpan.Zero))
            .Verifiable();

        // Act
        await implementation.ExecuteAsync().ShouldThrowAsync<RequestPostponedException>();
        await implementation.ExecuteAsync().ShouldThrowAsync<RequestPostponedException>();
    }
}