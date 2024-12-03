using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.SystemTests.OneAction.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.OneAction;

public class WorkflowFailedTests() : Base(nameof(WorkflowFailedTests))
{
    [Theory]
    [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
    [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
    [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
    public async Task Execute_01_Given_ActivityFailedException_Gives_WorkflowFailed(ActivityExceptionCategoryEnum category)
    {
        // Arrange
        var parameterValue = 3;
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(MyWorkflowContainer.ParameterNames.ParameterA, parameterValue);
        LogicMoq.Setup(l => l.ActionA(It.IsAny<IActivityAction<string>>(), parameterValue))
            .Throws(new ActivityFailedException(category, DataFixture.Create<string>(), DataFixture.Create<string>()))
            .Verifiable();

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<FulcrumCancelledException>();

        // Assert
        LogicMoq.Verify();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Failed.ToString());

        var activityInstances = (await RuntimeTables.ActivityInstance.SearchByWorkflowInstanceIdAsync(WorkflowInstanceId, 10)).ToArray();
        activityInstances.ShouldNotBeNull();
        activityInstances.Length.ShouldBe(1);
        var activityInstance = activityInstances[0];
        activityInstance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
        activityInstance.ExceptionCategory.ShouldBe(category.ToString());
    }

    [Fact]
    public async Task Execute_01_Given_UnexpectedException_Gives_WorkflowFailed()
    {
        // Arrange
        var parameterValue = 3;
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.CancelWorkflow;
        implementation.SetParameter(MyWorkflowContainer.ParameterNames.ParameterA, parameterValue);
        LogicMoq.Setup(l => l.ActionA(It.IsAny<IActivityAction<string>>(), parameterValue))
            .Throws(new ObjectCreationException())
            .Verifiable();

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<FulcrumCancelledException>();

        // Assert
        LogicMoq.Verify();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Failed.ToString());

        var activityInstances = (await RuntimeTables.ActivityInstance.SearchByWorkflowInstanceIdAsync(WorkflowInstanceId, 10)).ToArray();
        activityInstances.ShouldNotBeNull();
        activityInstances.Length.ShouldBe(1);
        var activityInstance = activityInstances[0];
        activityInstance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
        activityInstance.ExceptionCategory.ShouldBe(ActivityExceptionCategoryEnum.WorkflowImplementationError.ToString());
    }

}