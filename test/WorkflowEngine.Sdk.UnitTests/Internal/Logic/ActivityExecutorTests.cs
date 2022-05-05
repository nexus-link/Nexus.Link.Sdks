using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic
{
    public class ActivityExecutorTests
    {
        private readonly ActivityInformationMock _activityInformation;
        private readonly ActivityMock _activityMock;
        private readonly ActivityExecutor _executor;
        private readonly ActivityInformationMock _activityInformationResult;
        private readonly ActivityMock<int> _activityMockResult;
        private readonly ActivityExecutor _executorResult;

        public ActivityExecutorTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityExecutorTests));
            FulcrumApplication.Setup.SynchronousFastLogger = new ConsoleLogger();

            var workflowInformationMock = new WorkflowInformationMock(null);
            _activityInformation = new ActivityInformationMock(workflowInformationMock);
            _activityMock = new ActivityMock(_activityInformation);
            _executor = new ActivityExecutor(_activityMock);
            workflowInformationMock.Executor = _executor;

            var workflowInformationResultMock = new WorkflowInformationMock(null);
            _activityInformationResult = new ActivityInformationMock(workflowInformationResultMock);
            _activityMockResult = new ActivityMock<int>(_activityInformationResult, null);
            _executorResult = new ActivityExecutor(_activityMockResult);
            workflowInformationResultMock.Executor = _executorResult;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_MethodReturns_Gives_Success(bool withReturnValue)
        {
            // Arrange

            // Act
            if (withReturnValue)
            {
                await _executorResult.ExecuteWithReturnValueAsync(_ => Task.FromResult(10), null);
            }
            else
            {
                await _executor.ExecuteWithoutReturnValueAsync(_ => Task.CompletedTask);
            }

            // Assert
            if (withReturnValue)
            {
                _activityMockResult.Instance.State.ShouldBe(ActivityStateEnum.Success);
                _activityMockResult.SafeAlertExceptionCalled.ShouldBe(0);
            }
            else
            {
                _activityMock.Instance.State.ShouldBe(ActivityStateEnum.Success);
                _activityMock.SafeAlertExceptionCalled.ShouldBe(0);
            }
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
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_MethodThrowsAndStopping_Gives_StateFailed_Reported(bool withReturnValue)
        {
            // Arrange
            var executor = _executor;
            _activityMock.Options.FailUrgency = ActivityFailUrgencyEnum.Stopping;


            // Act
            if (withReturnValue)
            {
                await executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw new Exception(), null)
                    .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
            }
            else
            {
                await executor
                    .ExecuteWithoutReturnValueAsync(_ => throw new Exception())
                    .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
            }

            // Assert
            _activityMock.Instance.State.ShouldBe(ActivityStateEnum.Failed);
            _activityMock.SafeAlertExceptionCalled.ShouldBe(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_MethodThrowsAndStopping_Gives_ThrowsPostponed(bool withReturnValue)
        {
            // Arrange
            var executor = _executor;
            _activityMock.Options.FailUrgency = ActivityFailUrgencyEnum.Stopping;


            // Act
            WorkflowImplementationShouldNotCatchThisException outerException;
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
            outerException.InnerException.ShouldNotBeNull();
            outerException.InnerException.ShouldBeAssignableTo<RequestPostponedException>();
        }

        [Theory]
        [InlineData(ActivityFailUrgencyEnum.HandleLater)]
        [InlineData(ActivityFailUrgencyEnum.Ignore)]
        public async Task ExecuteWithReturnValue_Given_MethodThrows_NotStopping_Gives_Default(ActivityFailUrgencyEnum failUrgency)
        {
            // Arrange
            var executor = _executor;
            _activityMock.Options.FailUrgency = failUrgency;
            const int expectedValue = 10;

            // Act
            var actualValue = await executor.ExecuteWithReturnValueAsync(_ => throw new Exception(), _ => Task.FromResult(expectedValue));

            // Assert
            actualValue.ShouldBe(expectedValue);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_MethodThrowsRequestPostponed_Gives_RequestIdSet(bool withReturnValue)
        {
            // Arrange
            var executor = _executor;
            _activityMock.Options.FailUrgency = ActivityFailUrgencyEnum.Stopping;
            const string expectedRequestId = "D26D6803-03D2-4889-90E4-500B83839184";
            var requestPostponedException = new RequestPostponedException
            {
                WaitingForRequestIds = { expectedRequestId }
            };


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
            _activityMock.Instance.AsyncRequestId.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(false, typeof(FulcrumTryAgainException), null)]
        [InlineData(true, typeof(FulcrumTryAgainException), null)]
        [InlineData(false, typeof(FulcrumResourceLockedException), null)]
        [InlineData(true, typeof(FulcrumResourceLockedException), null)]
        [InlineData(false, typeof(FulcrumNotFoundException), null)] // TODO: Lars suggests that FulcrumNotFoundException should have IsRetryMeaningful = false
        [InlineData(true, typeof(FulcrumNotFoundException), null)] // TODO: Lars suggests that FulcrumNotFoundException should have IsRetryMeaningful = false
        [InlineData(false, typeof(FulcrumBusinessRuleException), ActivityExceptionCategoryEnum.BusinessError)]
        [InlineData(true, typeof(FulcrumBusinessRuleException), ActivityExceptionCategoryEnum.BusinessError)]
        [InlineData(false, typeof(FulcrumCancelledException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumCancelledException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumConflictException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumConflictException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumForbiddenAccessException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumForbiddenAccessException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumNotImplementedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumNotImplementedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumResourceException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumResourceException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumServiceContractException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumServiceContractException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumUnauthorizedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumUnauthorizedException), ActivityExceptionCategoryEnum.TechnicalError)]
        public async Task Execute_Given_ExpectedFulcrumException_Gives_CorrectInstanceUpdate(bool withReturnValue, Type exceptionType, ActivityExceptionCategoryEnum? expectedExceptionCategory)
        {
            // Arrange
            var executor = _executor;
            _activityMock.Options.FailUrgency = ActivityFailUrgencyEnum.Stopping;
            var exception = (FulcrumException)Activator.CreateInstance(exceptionType);
            FulcrumAssert.IsNotNull(exception, CodeLocation.AsString());
            var expectedTryAgain = exception!.IsRetryMeaningful;

            // Act
            WorkflowImplementationShouldNotCatchThisException outerException;
            if (withReturnValue)
            {
                outerException = await executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exception!, null)
                    .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
            }
            else
            {
                outerException = await executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exception!)
                    .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
            }

            // Assert
            outerException.InnerException.ShouldNotBeNull();
            var innerException = outerException.InnerException.ShouldBeAssignableTo<RequestPostponedException>();
            innerException.ShouldNotBeNull();
            innerException.TryAgain.ShouldBe(expectedTryAgain);
            if (expectedTryAgain)
            {
                _activityMock.Instance.State.ShouldBe(ActivityStateEnum.Waiting);
                _activityMock.Instance.ExceptionCategory.ShouldBeNull();
                FulcrumAssert.IsNull(expectedExceptionCategory, CodeLocation.AsString());
            }
            else
            {
                _activityMock.Instance.State.ShouldBe(ActivityStateEnum.Failed);
                _activityMock.Instance.ExceptionCategory.ShouldBe(expectedExceptionCategory);
            }
        }

        [Theory]
        [InlineData(false, typeof(FulcrumAssertionFailedException))]
        [InlineData(true, typeof(FulcrumAssertionFailedException))]
        [InlineData(false, typeof(FulcrumContractException))]
        [InlineData(true, typeof(FulcrumContractException))]
        public async Task Execute_Given_UnexpectedException_Gives_ImplementationError(bool withReturnValue, Type exceptionType)
        {
            // Arrange
            var executor = _executor;
            _activityMock.Options.FailUrgency = ActivityFailUrgencyEnum.Stopping;
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            FulcrumAssert.IsNotNull(exception, CodeLocation.AsString());

            // Act
            WorkflowImplementationShouldNotCatchThisException outerException;
            if (withReturnValue)
            {
                outerException = await executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exception!, null)
                    .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
            }
            else
            {
                outerException = await executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exception!)
                    .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
            }

            // Assert
            outerException.InnerException.ShouldNotBeNull();
            var innerException = outerException.InnerException.ShouldBeAssignableTo<RequestPostponedException>();
            innerException.ShouldNotBeNull();
            innerException.TryAgain.ShouldBe(false);
            _activityMock.Instance.State.ShouldBe(ActivityStateEnum.Failed);
            _activityMock.Instance.ExceptionCategory.ShouldBe(ActivityExceptionCategoryEnum.WorkflowImplementationError);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_TimeOut_Gives_Throws(bool withReturnValue)
        {
            // Arrange
            _activityInformation.Options.ActivityMaxExecutionTimeSpan = TimeSpan.Zero;
            _activityInformationResult.Options.ActivityMaxExecutionTimeSpan = TimeSpan.Zero;

            // Act
            if (withReturnValue)
            {
                await _executorResult.ExecuteWithReturnValueAsync(_ => Task.FromResult(10), null)
                    .ShouldThrowAsync<ActivityFailedException>();
            }
            else
            {
                await _executor.ExecuteWithoutReturnValueAsync(_ => Task.CompletedTask)
                    .ShouldThrowAsync<ActivityFailedException>();
            }

            // Assert
            if (withReturnValue)
            {
                _activityMockResult.SafeAlertExceptionCalled.ShouldBe(1);
                _activityMockResult.Instance.State.ShouldBe(ActivityStateEnum.Failed);
                _activityMockResult.Instance.ExceptionCategory.ShouldBe(ActivityExceptionCategoryEnum.MaxTimeReachedError);
            }
            else
            {
                _activityMock.SafeAlertExceptionCalled.ShouldBe(1);
                _activityMock.Instance.State.ShouldBe(ActivityStateEnum.Failed);
                _activityMock.Instance.ExceptionCategory.ShouldBe(ActivityExceptionCategoryEnum.MaxTimeReachedError);
            }
        }
    }
}

