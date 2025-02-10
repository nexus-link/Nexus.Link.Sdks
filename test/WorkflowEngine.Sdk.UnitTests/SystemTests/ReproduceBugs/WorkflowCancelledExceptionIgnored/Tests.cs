using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.SystemTests.ReproduceBugs.WorkflowCancelledExceptionIgnored.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.ReproduceBugs.WorkflowCancelledExceptionIgnored;

public class Tests() : TestBase(nameof(Tests))
{
    [Fact]
    public async Task Execute_GivenThrowsWorkflowFailedException_Gives_WorkflowFailed()
    {
        // Arrange
        var implementation = await WorkflowContainer.SelectImplementationAsync(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.Stopping;
        LogicMoq
            .Setup(l => l.Action1Async(It.IsAny<IActivityAction>()))
            .ThrowsAsync(new FulcrumResourceException($"Should be caught in the action and converted to a {nameof(WorkflowFailedException)}."))
            .Verifiable();
        LogicMoq
            .Setup(l => l.Action2Async(It.IsAny<IActivityAction>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await implementation.ExecuteAsync().ShouldThrowAsync<FulcrumCancelledException>();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(implementation.InstanceId.ToGuid());
        workflowInstance.State.ShouldBe(nameof(WorkflowStateEnum.Failed));
    }
}