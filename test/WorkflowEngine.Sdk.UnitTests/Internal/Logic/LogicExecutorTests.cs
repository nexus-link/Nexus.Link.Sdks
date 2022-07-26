using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic
{
    public class LogicExecutorTests
    {
        private readonly ActivityInformationMock _activityInformation;
        private readonly ActivityMock _activityMock;
        private readonly LogicExecutor _executor;
        private readonly WorkflowInformationMock _workflowInformationMock;

        public LogicExecutorTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(LogicExecutorTests));
            FulcrumApplication.Setup.SynchronousFastLogger = new ConsoleLogger();

            _workflowInformationMock = new WorkflowInformationMock(null, null);
            _activityInformation = new ActivityInformationMock(_workflowInformationMock);
            _activityMock = new ActivityMock(_activityInformation);
            _executor = new LogicExecutor(_activityMock);
            _workflowInformationMock.LogicExecutor = _executor;
        }

        [Fact]
        public async Task ExecuteWithoutReturnValue_Given_NormalMethod_Gives_MethodExecuted()
        {
            // Arrange
            var actuallyCalled = false;

            // Act
            await _executor.ExecuteWithoutReturnValueAsync(_ =>
            {
                actuallyCalled = true;
                return Task.CompletedTask;
            }, "Method");

            // Assert
            actuallyCalled.ShouldBe(true);
        }

        [Fact]
        public async Task ExecuteWithReturnValue_Given_MethodReturns_Gives_CorrectValue()
        {
            // Arrange
            const int expectedValue = 10;

            // Act
            var actualValue = await _executor.ExecuteWithReturnValueAsync(_ => Task.FromResult(expectedValue), "Method");

            // Assert
            actualValue.ShouldBe(expectedValue);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_MethodThrowsRequestPostponed_Gives_RequestIdSet(bool withResult)
        {
            // Arrange
            const string expectedRequestId = "D26D6803-03D2-4889-90E4-500B83839184";
            var requestPostponedException = new RequestPostponedException
            {
                WaitingForRequestIds = { expectedRequestId }
            };


            // Act
            RequestPostponedException exception;
            if (withResult)
            {
                exception = await _executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw requestPostponedException, "Method with return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }
            else
            {
                exception = await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw requestPostponedException, "Method without return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }

            // Assert
            exception.WaitingForRequestIds.ShouldNotBeNull();
            exception.WaitingForRequestIds.Count.ShouldBe(1);
            exception.WaitingForRequestIds.ShouldContain(expectedRequestId);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_ThrowsActivityException_Gives_ThrowsActivityFailedException(bool withReturnValue)
        {
            // Arrange
#pragma warning disable CS0618
            var exceptionToThrow = new ActivityException(ActivityExceptionCategoryEnum.BusinessError, "fail", "fail");
#pragma warning restore CS0618

            // Act & assert
            if (withReturnValue)
            {
                await _executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, "Method with return value")
                    .ShouldThrowAsync<ActivityFailedException>();
            }
            else
            {
                await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow!, "Method without return value")
                    .ShouldThrowAsync<ActivityFailedException>();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_ThrowsWorkflowFailedException_Gives_Rethrows(bool withReturnValue)
        {
            // Arrange
            var exceptionToThrow = new WorkflowFailedException(ActivityExceptionCategoryEnum.BusinessError, "fail", "fail");

            // Act & assert
            WorkflowFailedException thrownException;
            if (withReturnValue)
            {
                thrownException = await _executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, "Method with return value")
                    .ShouldThrowAsync<WorkflowFailedException>();
            }
            else
            {
                thrownException = await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow, "Method without return value")
                    .ShouldThrowAsync<WorkflowFailedException>();
            }
            thrownException.ShouldBe(exceptionToThrow);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_ThrowsActivityFailedException_Gives_Rethrows(bool withReturnValue)
        {
            // Arrange
            var exceptionToThrow = new ActivityFailedException(ActivityExceptionCategoryEnum.BusinessError, "fail", "fail");

            // Act & assert
            ActivityFailedException thrownException;
            if (withReturnValue)
            {
                thrownException = await _executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, "Method with return value")
                    .ShouldThrowAsync<ActivityFailedException>();
            }
            else
            {
                thrownException = await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow, "Method without return value")
                    .ShouldThrowAsync<ActivityFailedException>();
            }
            thrownException.ShouldBe(exceptionToThrow);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_ThrowsRequestPostponedException_Gives_Rethrows(bool withReturnValue)
        {
            // Arrange
            var exceptionToThrow = new RequestPostponedException();

            // Act & assert
            RequestPostponedException thrownException;
            if (withReturnValue)
            {
                thrownException = await _executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, "Method with return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }
            else
            {
                thrownException = await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow!, "Method without return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }
            thrownException.ShouldBe(exceptionToThrow);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_ThrowsOperationCancelledException_Gives_ThrowsRequestPostponed(bool withReturnValue)
        {
            // Arrange
            var exceptionToThrow = new OperationCanceledException();

            // Act & assert
            if (withReturnValue)
            {
                await _executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, "Method with return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }
            else
            {
                await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow!, "Method without return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_ThrowsWorkflowImplementationShouldNotCatchThisException_Gives_RethrowsInnerException(bool withReturnValue)
        {
            // Arrange
            var expectedInnerException = new RequestPostponedException();
            var exceptionToThrow = new WorkflowImplementationShouldNotCatchThisException(expectedInnerException);

            // Act & assert
            RequestPostponedException thrownException;
            if (withReturnValue)
            {
                thrownException = await _executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, "Method with return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }
            else
            {
                thrownException = await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow!, "Method without return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }
            thrownException.ShouldBe(expectedInnerException);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_ThrowsWorkflowImplementationShouldNotCatchThisExceptionWithBadInnerException_Gives_ThrowsApplicationFailedException(bool withReturnValue)
        {
            // Arrange
            var innerException = new ApplicationException();
            var exceptionToThrow = new WorkflowImplementationShouldNotCatchThisException(innerException);

            // Act & assert
            if (withReturnValue)
            {
                await _executor
                       .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow, "Method with return value")
                       .ShouldThrowAsync<ActivityFailedException>();
            }
            else
            {
                await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow!, "Method without return value")
                    .ShouldThrowAsync<ActivityFailedException>();
            }
        }

        [Theory]
        [InlineData(false, typeof(FulcrumTryAgainException), typeof(RequestPostponedException), null)]
        [InlineData(true, typeof(FulcrumTryAgainException), typeof(RequestPostponedException), null)]
        [InlineData(false, typeof(FulcrumResourceLockedException), typeof(RequestPostponedException), null)]
        [InlineData(true, typeof(FulcrumResourceLockedException), typeof(RequestPostponedException), null)]
        [InlineData(false, typeof(FulcrumNotFoundException), typeof(RequestPostponedException), null)] // TODO: Lars suggests that FulcrumNotFoundException should have IsRetryMeaningful = false
        [InlineData(true, typeof(FulcrumNotFoundException), typeof(RequestPostponedException), null)] // TODO: Lars suggests that FulcrumNotFoundException should have IsRetryMeaningful = false
        [InlineData(false, typeof(FulcrumBusinessRuleException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.BusinessError)]
        [InlineData(true, typeof(FulcrumBusinessRuleException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.BusinessError)]
        [InlineData(false, typeof(FulcrumCancelledException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumCancelledException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumConflictException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumConflictException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumForbiddenAccessException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumForbiddenAccessException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumNotImplementedException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumNotImplementedException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumResourceException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumResourceException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumServiceContractException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumServiceContractException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(false, typeof(FulcrumUnauthorizedException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(true, typeof(FulcrumUnauthorizedException), typeof(ActivityFailedException), ActivityExceptionCategoryEnum.TechnicalError)]
        public async Task Execute_Given_ThrowsFulcrumExceptionWithTryAgain_Gives_ThrowsCorrectConversion(bool withReturnValue, Type thrownExceptionType, Type expectedExceptionType, ActivityExceptionCategoryEnum? expectedCategory)
        {
            // Arrange
            var exceptionToThrow = (FulcrumException)Activator.CreateInstance(thrownExceptionType);
            FulcrumAssert.IsNotNull(exceptionToThrow, CodeLocation.AsString());
            var expectedTryAgain = exceptionToThrow!.IsRetryMeaningful;

            // Act
            Exception thrownException;
            if (withReturnValue)
            {
                thrownException = await _executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow!, "Method with return value")
                    .ShouldThrowAsync<Exception>();
            }
            else
            {
                thrownException = await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow!, "Method without return value")
                    .ShouldThrowAsync<Exception>();
            }

            // Assert
            thrownException.ShouldBeOfType(expectedExceptionType);
            if (thrownException is ActivityFailedException activityFailedException)
            {
                expectedCategory.ShouldNotBeNull();
                activityFailedException.ExceptionCategory.ShouldBe(expectedCategory.Value);
            }
        }

        [Theory]
        [InlineData(false, typeof(FulcrumAssertionFailedException))]
        [InlineData(true, typeof(FulcrumAssertionFailedException))]
        [InlineData(false, typeof(FulcrumContractException))]
        [InlineData(true, typeof(FulcrumContractException))]
        [InlineData(false, typeof(ArgumentException))]
        [InlineData(true, typeof(ArgumentException))]
        public async Task Execute_Given_UnexpectedException_Gives_ImplementationError(bool withReturnValue, Type exceptionType)
        {
            // Arrange
            var exceptionToThrow = (Exception)Activator.CreateInstance(exceptionType);
            FulcrumAssert.IsNotNull(exceptionToThrow, CodeLocation.AsString());

            // Act
            ActivityFailedException thrownException;
            if (withReturnValue)
            {
                thrownException = await _executor
                    .ExecuteWithReturnValueAsync<int>(_ => throw exceptionToThrow!, "Method with return value")
                    .ShouldThrowAsync<ActivityFailedException>();
            }
            else
            {
                thrownException = await _executor
                    .ExecuteWithoutReturnValueAsync(_ => throw exceptionToThrow!, "Method without return value")
                    .ShouldThrowAsync<ActivityFailedException>();
            }

            // Assert
            thrownException.ExceptionCategory.ShouldBe(ActivityExceptionCategoryEnum.WorkflowImplementationError);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Execute_Given_TimeOut_Gives_ThrowsRequestPostponed(bool withReturnValue)
        {
            // Arrange
            RequestPostponedException thrownException;
            _workflowInformationMock.ReducedTimeCancellationToken = new CancellationTokenSource(TimeSpan.Zero).Token;

            // Act
            if (withReturnValue)
            {
                thrownException = await _executor
                    .ExecuteWithReturnValueAsync(_ => Task.FromResult(10), "Method with return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }
            else
            {
                thrownException = await _executor
                    .ExecuteWithoutReturnValueAsync(_ => Task.CompletedTask, "Method without return value")
                    .ShouldThrowAsync<RequestPostponedException>();
            }

            // Assert
            thrownException.TryAgain.ShouldBe(true);
        }
    }
}

