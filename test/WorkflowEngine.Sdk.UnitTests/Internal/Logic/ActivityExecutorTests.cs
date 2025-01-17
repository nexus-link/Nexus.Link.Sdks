using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic;

public class ActivityExecutorTests
{
    private readonly ActivityInformationMock _activityInformation;
    private readonly ActivityMock _activityMock;
    private readonly ActivityExecutor _executor;
    private readonly ActivityInformationMock _activityInformationResult;
    private readonly ActivityMock<int> _activityMockResult;
    private readonly ActivityExecutor _executorResult;
    private readonly WorkflowInformationMock _workflowInformationMock;
    private readonly WorkflowInformationMock _workflowInformationResultMock;

    public ActivityExecutorTests()
    {
        FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityExecutorTests));
        FulcrumApplication.Setup.SynchronousFastLogger = new ConsoleLogger();

        _workflowInformationMock = new WorkflowInformationMock(null, null);
        _activityInformation = new ActivityInformationMock(_workflowInformationMock);
        _activityMock = new ActivityMock(_activityInformation);
        _executor = new ActivityExecutor(_activityMock);
        _workflowInformationMock.ActivityExecutor = _executor;

        _workflowInformationResultMock = new WorkflowInformationMock(null, null);
        _activityInformationResult = new ActivityInformationMock(_workflowInformationResultMock);
        _activityMockResult = new ActivityMock<int>(_activityInformationResult, null);
        _executorResult = new ActivityExecutor(_activityMockResult);
        _workflowInformationResultMock.ActivityExecutor = _executorResult;
    }

    [Fact]
    public async Task Execute_Given_FireAndForgetAndMethodThrowsRequestPostponed_Gives_Success()
    {
        // Arrange
        const string expectedRequestId = "D26D6803-03D2-4889-90E4-500B83839184";
        var requestPostponedException = new ActivityWaitsForRequestException(expectedRequestId);
        WorkflowStatic.Context.ExecutionBackgroundStyle = ActivityAction.BackgroundStyleEnum.FireAndForget;

        // Act
        await _executor
            .ExecuteWithoutReturnValueAsync(_ => throw requestPostponedException);

        // Assert

    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_Given_TotalExecutionTimePassed_Gives_MaxTimeReachedError(bool withReturnValue)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
        }
#pragma warning disable CS0618
        activity.Options.ActivityMaxExecutionTimeSpan = TimeSpan.Zero;
#pragma warning restore CS0618
        WorkflowImplementationShouldNotCatchThisException outerException;

        // Act
        if (withReturnValue)
        {
            outerException = await executor
                .ExecuteWithReturnValueAsync(_ => Task.FromResult(10), null)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }
        else
        {
            outerException = await executor
                .ExecuteWithoutReturnValueAsync(_ => Task.CompletedTask)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }

        // Assert
        var innerException = outerException.InnerException;
        innerException.ShouldBeAssignableTo<RequestPostponedException>();
        activity.Instance.State.ShouldBe(ActivityStateEnum.Failed);
        var exception = activity.GetException();
        exception.ExceptionCategory.ShouldBe(ActivityExceptionCategoryEnum.MaxTimeReachedError);
        activity.SafeAlertExceptionCalled.ShouldBe(1);
        activity.Instance.AsyncRequestId.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_Given_ReducedTimePassed_Gives_RequestPostponed(bool withReturnValue)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        WorkflowInformationMock workflowInformation;
        if (withReturnValue)
        {
            workflowInformation = _workflowInformationResultMock;
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            workflowInformation = _workflowInformationMock;
            activity = _activityMock;
            executor = _executor;
        }
        workflowInformation.ReducedTimeCancellationToken = new CancellationTokenSource(TimeSpan.Zero).Token;
        WorkflowImplementationShouldNotCatchThisException outerException;

        // Act
        if (withReturnValue)
        {
            outerException = await executor
                .ExecuteWithReturnValueAsync(_ => Task.FromResult(10), null)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }
        else
        {
            outerException = await executor
                .ExecuteWithoutReturnValueAsync(_ => Task.CompletedTask)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }

        // Assert
        var innerException = outerException.InnerException;
        innerException.ShouldBeAssignableTo<RequestPostponedException>();
        activity.Instance.State.ShouldBe(ActivityStateEnum.Waiting);
        activity.SafeAlertExceptionCalled.ShouldBe(0);
        activity.Instance.AsyncRequestId.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_Given_ThrowsWorkflowFailedException_Gives_Rethrows(bool withReturnValue)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
        }
        var exceptionToThrow = new WorkflowFailedException(ActivityExceptionCategoryEnum.BusinessError, "fail", "fail");
        WorkflowImplementationShouldNotCatchThisException outerException;

        // Act & assert
        if (withReturnValue)
        {
            outerException = await executor
                .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, null)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }
        else
        {
            outerException = await executor
                .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }

        outerException.InnerException.ShouldNotBeNull();
        var innerException = outerException.InnerException;
        innerException.ShouldBe(exceptionToThrow);
        activity.Instance.State.ShouldBe(ActivityStateEnum.Failed);
        var exception = activity.GetException();
        exception.ExceptionCategory.ShouldBe(exceptionToThrow.ExceptionCategory);
        activity.SafeAlertExceptionCalled.ShouldBe(1);
        activity.Instance.AsyncRequestId.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_Given_ThrowsActivityFailedException_Gives_Rethrows(bool withReturnValue)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
        }
        var exceptionToThrow = new ActivityFailedException(ActivityExceptionCategoryEnum.BusinessError, "fail", "fail");
        WorkflowImplementationShouldNotCatchThisException outerException;

        // Act & assert
        if (withReturnValue)
        {
            outerException = await executor
                .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, null)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }
        else
        {
            outerException = await executor
                .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }

        outerException.InnerException.ShouldNotBeNull();
        var innerException = outerException.InnerException;
        innerException.ShouldBeAssignableTo<RequestPostponedException>();
        activity.Instance.State.ShouldBe(ActivityStateEnum.Failed);
        var exception = activity.GetException();
        exception.ExceptionCategory.ShouldBe(exceptionToThrow.ExceptionCategory);
        activity.SafeAlertExceptionCalled.ShouldBe(1);
        activity.Instance.AsyncRequestId.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_Given_ThrowsRequestPostponedException_Gives_Rethrows(bool withReturnValue)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
        }
        var exceptionToThrow = new ActivityPostponedException(null);
        WorkflowImplementationShouldNotCatchThisException outerException;

        // Act & assert
        if (withReturnValue)
        {
            outerException = await executor
                .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, null)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }
        else
        {
            outerException = await executor
                .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }

        outerException.InnerException.ShouldNotBeNull();
        var innerException = outerException.InnerException;
        innerException.ShouldBeOfType<ActivityPostponedException>();
        activity.Instance.State.ShouldBe(ActivityStateEnum.Waiting);
        activity.SafeAlertExceptionCalled.ShouldBe(0);
        activity.Instance.AsyncRequestId.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_Given_ThrowsOperationCancelledException_Gives_ThrowsRequestPostponed(bool withReturnValue)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
        }
        var exceptionToThrow = new OperationCanceledException();
        WorkflowImplementationShouldNotCatchThisException outerException;

        // Act & assert
        if (withReturnValue)
        {
            outerException = await executor
                .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, null)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }
        else
        {
            outerException = await executor
                .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }

        outerException.InnerException.ShouldNotBeNull();
        var innerException = outerException.InnerException;
        innerException.ShouldBeOfType<ActivityTemporaryFailureException>();
        activity.Instance.State.ShouldBe(ActivityStateEnum.Waiting);
        activity.SafeAlertExceptionCalled.ShouldBe(0);
        activity.Instance.AsyncRequestId.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_Given_MethodReturns_Gives_Success(bool withReturnValue)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
        }

        // Act
        if (withReturnValue)
        {
            await executor.ExecuteWithReturnValueAsync(_ => Task.FromResult(10), null);
        }
        else
        {
            await executor.ExecuteWithoutReturnValueAsync(_ => Task.CompletedTask);
        }

        // Assert
        activity.Instance.State.ShouldBe(ActivityStateEnum.Success);
        activity.SafeAlertExceptionCalled.ShouldBe(0);
        activity.Instance.AsyncRequestId.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_Given_CallAgain_Gives_NoMethodCall(bool withReturnValue)
    {
        // Arrange
        int calls = 0;
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
            await executor.ExecuteWithReturnValueAsync(_ =>
            {
                calls++;
                return Task.FromResult(10);
            }, null);
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
            await executor.ExecuteWithoutReturnValueAsync(_ =>
            {
                calls++;
                return Task.CompletedTask;
            });
        }

        // Act
        if (withReturnValue)
        {
            await executor.ExecuteWithReturnValueAsync(_ =>
            {
                calls++;
                return Task.FromResult(10);
            }, null);
        }
        else
        {
            await executor.ExecuteWithoutReturnValueAsync(_ =>
            {
                calls++;
                return Task.CompletedTask;
            });
        }

        // Assert
        activity.Instance.State.ShouldBe(ActivityStateEnum.Success);
        activity.SafeAlertExceptionCalled.ShouldBe(0);
        activity.Instance.AsyncRequestId.ShouldBeNull();
        calls.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteWithReturnValue_Given_MethodReturns_Gives_CorrectValue()
    {
        // Arrange
        var executor = _executorResult;
        const int expectedValue = 10;

        // Act
        var actualValue = await executor.ExecuteWithReturnValueAsync(_ => Task.FromResult(expectedValue), null);

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(false, ActivityFailUrgencyEnum.CancelWorkflow)]
    [InlineData(true, ActivityFailUrgencyEnum.CancelWorkflow)]
    [InlineData(false, ActivityFailUrgencyEnum.Stopping)]
    [InlineData(true, ActivityFailUrgencyEnum.Stopping)]
    public async Task Execute_Given_MethodThrowsAndNotIgnorable_Gives_StateFailedAndReported(bool withReturnValue, ActivityFailUrgencyEnum failUrgency)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
        }
        activity.Options.FailUrgency = failUrgency;
        WorkflowImplementationShouldNotCatchThisException outerException;

        // Act
        if (withReturnValue)
        {
            outerException = await executor
                .ExecuteWithReturnValueAsync<int>(_ => throw new Exception(), null)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }
        else
        {
            outerException = await executor
                .ExecuteWithoutReturnValueAsync(_ => throw new Exception())
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }

        // Assert
        activity.Instance.State.ShouldBe(ActivityStateEnum.Failed);
        activity.SafeAlertExceptionCalled.ShouldBe(1);
        activity.Instance.AsyncRequestId.ShouldBeNull();
        if (failUrgency == ActivityFailUrgencyEnum.Stopping)
        {
            outerException.InnerException.ShouldBeAssignableTo<ActivityPostponedException>();
        }
        else
        {
            outerException.InnerException.ShouldBeAssignableTo<WorkflowFailedException>();
        }
    }

    [Theory]
    [InlineData(ActivityFailUrgencyEnum.HandleLater)]
    [InlineData(ActivityFailUrgencyEnum.Ignore)]
    public async Task ExecuteWithReturnValue_Given_MethodThrows_NotStopping_Gives_Default(ActivityFailUrgencyEnum failUrgency)
    {
        // Arrange
        var executor = _executorResult;
        _activityMockResult.Options.FailUrgency = failUrgency;
        const int expectedValue = 10;

        // Act
        var actualValue = await executor.ExecuteWithReturnValueAsync(_ => throw new Exception(), _ => Task.FromResult(expectedValue));

        // Assert
        actualValue.ShouldBe(expectedValue);
        _activityMockResult.Instance.State.ShouldBe(ActivityStateEnum.Failed);
        _activityMockResult.SafeAlertExceptionCalled.ShouldBe(1);
        _activityMockResult.Instance.AsyncRequestId.ShouldBeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_Given_MethodThrowsRequestPostponed_Gives_RequestIdSet(bool withReturnValue)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
        }
        activity.Options.FailUrgency = ActivityFailUrgencyEnum.Stopping;
        const string expectedRequestId = "D26D6803-03D2-4889-90E4-500B83839184";
        var requestPostponedException = new ActivityWaitsForRequestException(expectedRequestId);


        // Act
        if (withReturnValue)
        {
            await executor
                .ExecuteWithReturnValueAsync<int>(_ => throw requestPostponedException, null)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }
        else
        {
            await executor
                .ExecuteWithoutReturnValueAsync(_ => throw requestPostponedException)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }

        // Assert
        activity.Instance.State.ShouldBe(ActivityStateEnum.Waiting);
        activity.SafeAlertExceptionCalled.ShouldBe(0);
        activity.Instance.AsyncRequestId.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(false, typeof(Exception))]
    [InlineData(true, typeof(Exception))]
    [InlineData(false, typeof(ArgumentException))]
    [InlineData(true, typeof(ArgumentException))]
    [InlineData(false, typeof(FulcrumTryAgainException))]
    [InlineData(true, typeof(FulcrumTryAgainException))]
    [InlineData(false, typeof(FulcrumResourceLockedException))]
    [InlineData(true, typeof(FulcrumResourceLockedException))]
    [InlineData(false, typeof(FulcrumNotFoundException))]
    [InlineData(true, typeof(FulcrumNotFoundException))]
    [InlineData(false, typeof(FulcrumBusinessRuleException))]
    [InlineData(true, typeof(FulcrumBusinessRuleException))]
    [InlineData(false, typeof(FulcrumCancelledException))]
    [InlineData(true, typeof(FulcrumCancelledException))]
    [InlineData(false, typeof(FulcrumConflictException))]
    [InlineData(true, typeof(FulcrumConflictException))]
    [InlineData(false, typeof(FulcrumForbiddenAccessException))]
    [InlineData(true, typeof(FulcrumForbiddenAccessException))]
    [InlineData(false, typeof(FulcrumNotImplementedException))]
    [InlineData(true, typeof(FulcrumNotImplementedException))]
    [InlineData(false, typeof(FulcrumResourceException))]
    [InlineData(true, typeof(FulcrumResourceException))]
    [InlineData(false, typeof(FulcrumServiceContractException))]
    [InlineData(true, typeof(FulcrumServiceContractException))]
    [InlineData(false, typeof(FulcrumUnauthorizedException))]
    [InlineData(true, typeof(FulcrumUnauthorizedException))]
    [InlineData(false, typeof(FulcrumAssertionFailedException))]
    [InlineData(true, typeof(FulcrumAssertionFailedException))]
    [InlineData(false, typeof(FulcrumContractException))]
    [InlineData(true, typeof(FulcrumContractException))]
    public async Task Execute_Given_UnexpectedException_Gives_WorkflowCapabilityError(bool withReturnValue, Type exceptionType)
    {
        // Arrange
        ActivityExecutor executor;
        ActivityMock activity;
        if (withReturnValue)
        {
            activity = _activityMockResult;
            executor = _executorResult;
        }
        else
        {
            activity = _activityMock;
            executor = _executor;
        }
        activity.Options.FailUrgency = ActivityFailUrgencyEnum.Stopping;
        var exceptionToThrow = (Exception) Activator.CreateInstance(exceptionType);
        FulcrumAssert.IsNotNull(exceptionToThrow, CodeLocation.AsString());

        // Act
        WorkflowImplementationShouldNotCatchThisException outerException;
        if (withReturnValue)
        {
            outerException = await executor
                .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow!, null)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }
        else
        {
            outerException = await executor
                .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow!)
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
        }

        // Assert
        outerException.InnerException.ShouldNotBeNull();
        var innerException = outerException.InnerException.ShouldBeAssignableTo<RequestPostponedException>();
        innerException.ShouldNotBeNull();
        activity.Instance.State.ShouldBe(ActivityStateEnum.Failed);
        var exception = activity.GetException();
        exception.ExceptionCategory.ShouldBe(ActivityExceptionCategoryEnum.WorkflowCapabilityError);
    }
}