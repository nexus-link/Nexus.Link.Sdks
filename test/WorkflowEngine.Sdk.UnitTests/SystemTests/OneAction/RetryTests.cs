using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.SystemTests.OneAction.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.OneAction;

public class RetryTests : Base
{

    public RetryTests() :base(nameof(RetryTests))
    {
    }

    [Theory]
    [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
    [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
    [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
    public async Task Execute_01_Given_ActivityFailedException_Gives_ThrowsRequestPostponedException(ActivityExceptionCategoryEnum category)
    {
        // Arrange
        var parameterValue = 3;
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.Stopping;
        implementation.SetParameter(MyWorkflowContainer.ParameterNames.ParameterA, parameterValue);
        LogicMoq.Setup(l => l.ActionA(It.IsAny<IActivityAction<string>>(), parameterValue))
            .Throws(new ActivityFailedException(category, DataFixture.Create<string>(), DataFixture.Create<string>()))
            .Verifiable();

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.ShouldNotBeNull();
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Halted.ToString());

        var activityInstances = (await RuntimeTables.ActivityInstance.SearchByWorkflowInstanceIdAsync(WorkflowInstanceId, 10)).ToArray();
        activityInstances.ShouldNotBeNull();
        activityInstances.Length.ShouldBe(1);
        var activityInstance = activityInstances[0];
        activityInstance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
        activityInstance.ExceptionCategory.ShouldBe(category.ToString());
    }

    [Theory]
    [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
    [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
    [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
    public async Task Execute_02_Given_CallRetryAfterActivityFailedException_Gives_ActivyReset(ActivityExceptionCategoryEnum category)
    {
        //
        // Arrange
        //

        // Prepare for execute
        var parameterValue = 3;
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.Stopping;
        implementation.SetParameter(MyWorkflowContainer.ParameterNames.ParameterA, parameterValue);
        LogicMoq.Setup(l => l.ActionA(It.IsAny<IActivityAction<string>>(), parameterValue))
            .Throws(new ActivityFailedException(category, DataFixture.Create<string>(), DataFixture.Create<string>()));

        // Execute
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Verify execute
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Halted.ToString());
        var activityInstances = (await RuntimeTables.ActivityInstance.SearchByWorkflowInstanceIdAsync(WorkflowInstanceId, 10)).ToArray();
        var activityInstance = activityInstances[0];
        var activityInstanceId = activityInstance.Id;
        activityInstance.State.ShouldBe(ActivityStateEnum.Failed.ToString());

        // Prepare for retry
        AsyncRequestMgmtMock.RequestServiceMoq.Setup(rs => rs.RetryAsync(WorkflowInstanceId.ToGuidString(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        //
        // Act
        //
        await WorkflowMgmtCapability.Activity.RetryAsync(activityInstance.Id.ToString());

        //
        // Assert
        //
        AsyncRequestMgmtMock.RequestServiceMoq.Verify();
        activityInstance = await RuntimeTables.ActivityInstance.ReadAsync(activityInstanceId);
        activityInstance.State.ShouldBe(ActivityStateEnum.Waiting.ToString());
        activityInstance.ExceptionCategory.ShouldBeNull();
        activityInstance.ExceptionTechnicalMessage.ShouldBeNull();
        activityInstance.ExceptionFriendlyMessage.ShouldBeNull();
    }

    [Theory]
    [InlineData(WorkflowStateEnum.Waiting)]
    [InlineData(WorkflowStateEnum.Executing)]
    [InlineData(WorkflowStateEnum.Halting)]
    public async Task Execute_03_Given_RetryOnNotHalted_Gives_ThrowsFulcrumBusinessError(WorkflowStateEnum workflowState)
    {
        //
        // Arrange
        //

        // Prepare for execute
        var parameterValue = 3;
        var category = ActivityExceptionCategoryEnum.BusinessError;
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.Stopping;
        implementation.SetParameter(MyWorkflowContainer.ParameterNames.ParameterA, parameterValue);
        LogicMoq.Setup(l => l.ActionA(It.IsAny<IActivityAction<string>>(), parameterValue))
            .Throws(new ActivityFailedException(category, DataFixture.Create<string>(), DataFixture.Create<string>()));

        // Execute
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Verify execute
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Halted.ToString());
        var activityInstances = (await RuntimeTables.ActivityInstance.SearchByWorkflowInstanceIdAsync(WorkflowInstanceId, 10)).ToArray();
        var activityInstance = activityInstances[0];
        var activityInstanceId = activityInstance.Id;
        activityInstance.State.ShouldBe(ActivityStateEnum.Failed.ToString());

        // Prepare for retry
        AsyncRequestMgmtMock.RequestServiceMoq.Setup(rs => rs.RetryAsync(WorkflowInstanceId.ToGuidString(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        workflowInstance.State = workflowState.ToString();
        await RuntimeTables.WorkflowInstance.UpdateAndReturnAsync(WorkflowInstanceId, workflowInstance);

        //
        // Act & Assert
        //
        await WorkflowMgmtCapability.Activity.RetryAsync(activityInstance.Id.ToString())
            .ShouldThrowAsync<FulcrumBusinessRuleException>();
    }

    [Theory]
    [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
    [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
    [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
    public async Task Execute_04_Given_ExecuteAfterRetry_Gives_Success(ActivityExceptionCategoryEnum category)
    {
        //
        // Arrange
        //

        // Prepare for execute
        var parameterValue = 3;
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.Stopping;
        implementation.SetParameter(MyWorkflowContainer.ParameterNames.ParameterA, parameterValue);
        LogicMoq.Setup(l => l.ActionA(It.IsAny<IActivityAction<string>>(), parameterValue))
            .Throws(new ActivityFailedException(category, DataFixture.Create<string>(), DataFixture.Create<string>()));

        // Execute
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Verify execute
        var workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Halted.ToString());
        var activityInstances = (await RuntimeTables.ActivityInstance.SearchByWorkflowInstanceIdAsync(WorkflowInstanceId, 10)).ToArray();
        var activityInstance = activityInstances[0];
        var activityInstanceId = activityInstance.Id;
        activityInstance.State.ShouldBe(ActivityStateEnum.Failed.ToString());

        // Prepare for retry
        AsyncRequestMgmtMock.RequestServiceMoq.Setup(rs => rs.RetryAsync(WorkflowInstanceId.ToGuidString(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Retry
        await WorkflowMgmtCapability.Activity.RetryAsync(activityInstance.Id.ToString());
        
        // Verify retry
        AsyncRequestMgmtMock.RequestServiceMoq.Verify();
        workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Waiting.ToString());
        activityInstance = await RuntimeTables.ActivityInstance.ReadAsync(activityInstanceId);
        activityInstance.State.ShouldBe(ActivityStateEnum.Waiting.ToString());

        // Prepare for execute
        LogicMoq.Setup(l => l.ActionA(It.IsAny<IActivityAction<string>>(), parameterValue))
            .Returns(parameterValue.ToString());
        implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        implementation.DefaultActivityOptions.FailUrgency = ActivityFailUrgencyEnum.Stopping;
        implementation.SetParameter(MyWorkflowContainer.ParameterNames.ParameterA, parameterValue);

        //
        // Act
        //
        await implementation.ExecuteAsync();

        //
        // Assert
        //
        workflowInstance = await RuntimeTables.WorkflowInstance.ReadAsync(WorkflowInstanceId);
        workflowInstance.State.ShouldBe(WorkflowStateEnum.Success.ToString());
    }
}