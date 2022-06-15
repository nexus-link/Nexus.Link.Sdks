using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities
{
    public class ActivityLockTests : ActivityTestsBase
    {
        private readonly Mock<ISemaphoreSupport> _semaphoreSupportMock;

        public ActivityLockTests() : base(nameof(ActivityLockTests))
        {
            _semaphoreSupportMock = new Mock<ISemaphoreSupport>();
            _semaphoreSupportMock.SetupGet(s => s.IsThrottle).Returns(false);
            _semaphoreSupportMock.SetupGet(s => s.Limit).Returns(1);
        }

        #region No return value
        [Fact]
        public async Task Execute_Given_Normal_Gives_ActivityExecutorActivated()
        {
            // Arrange
            var activity = new ActivityLock(_activityInformationMock, _semaphoreSupportMock.Object);

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(activity.LockOrThrottleAsync, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LockOrThrottle_Given_LockAcquired_Gives_CallsThen()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToGuidString());
            var activity = new ActivityLock(_activityInformationMock, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.CompletedTask);
            activity.Else((_, _) => Task.CompletedTask);

            // Act
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Then");
            logicExecutor.ExecuteWithoutReturnValueCounter["Then"].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Else");
        }

        [Fact]
        public async Task LockOrThrottle_Given_LockAcquiredAndRepeated_Gives_CallsLockAndThenAgain()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToGuidString());
            var activity = new ActivityLock(_activityInformationMock, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.CompletedTask);
            activity.Else((_, _) => Task.CompletedTask);
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Act
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(2);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Then");
            logicExecutor.ExecuteWithoutReturnValueCounter["Then"].ShouldBe(2);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Else");
        }

        [Fact]
        public async Task LockOrThrottle_Given_LockFailed_Gives_CallsElse()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestPostponedException("Already locked"));
            var activity = new ActivityLock(_activityInformationMock, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.CompletedTask);
            activity.Else((_, _) => Task.CompletedTask);

            // Act
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Else");
            logicExecutor.ExecuteWithoutReturnValueCounter["Else"].ShouldBe(1);
        }

        [Fact]
        public async Task LockOrThrottle_Given_LockFailedAndRepeated_Gives_NoLockRetryButCallsElse()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestPostponedException("Already locked"));
            var activity = new ActivityLock(_activityInformationMock, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.CompletedTask);
            activity.Else((_, _) => Task.CompletedTask);
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Act
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Else");
            logicExecutor.ExecuteWithoutReturnValueCounter["Else"].ShouldBe(2);
        }

        [Fact]
        public async Task LockOrThrottle_Given_LockFailedAndNoElse_Gives_Throws()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestPostponedException("Already locked"));
            var activity = new ActivityLock(_activityInformationMock, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.CompletedTask);

            // Act
            await Should.ThrowAsync<RequestPostponedException>(activity.LockOrThrottleAsync(CancellationToken.None));

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Else");
        }

        [Fact]
        public async Task LockOrThrottle_Given_LockFailedAndNoElseAndRepeated_Gives_Throws()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestPostponedException("Already locked"));
            var activity = new ActivityLock(_activityInformationMock, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.CompletedTask);
            await Should.ThrowAsync<RequestPostponedException>(activity.LockOrThrottleAsync(CancellationToken.None));

            // Act
            await Should.ThrowAsync<RequestPostponedException>(activity.LockOrThrottleAsync(CancellationToken.None));

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(2);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldNotContainKey("Else");
        }
        #endregion

        #region Return value
        [Fact]
        public async Task RV_Execute_Given_Normal_Gives_ActivityExecutorActivated()
        {
            // Arrange
            var activity = new ActivityLock<int>(_activityInformationMock, null, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.FromResult(1));
            activity.Else((_, _) => Task.FromResult(2));

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(e => e.ExecuteWithReturnValueAsync(activity.LockOrThrottleAsync, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RV_LockOrThrottle_Given_LockAcquired_Gives_CallsThen()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToGuidString());
            var activity = new ActivityLock<int>(_activityInformationMock, null, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.FromResult(1));
            activity.Else((_, _) => Task.FromResult(2));

            // Act
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Then");
            logicExecutor.ExecuteWithReturnValueCounter["Then"].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Else");
        }

        [Fact]
        public async Task RV_LockOrThrottle_Given_LockAcquiredAndRepeated_Gives_CallsLockAndThenAgain()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToGuidString());
            var activity = new ActivityLock<int>(_activityInformationMock, null, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.FromResult(1));
            activity.Else((_, _) => Task.FromResult(2));
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Act
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(2);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Then");
            logicExecutor.ExecuteWithReturnValueCounter["Then"].ShouldBe(2);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Else");
        }

        [Fact]
        public async Task RV_LockOrThrottle_Given_LockFailed_Gives_CallsElse()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestPostponedException("Already locked"));
            var activity = new ActivityLock<int>(_activityInformationMock, null, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.FromResult(1));
            activity.Else((_, _) => Task.FromResult(2));

            // Act
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Else");
            logicExecutor.ExecuteWithReturnValueCounter["Else"].ShouldBe(1);
        }

        [Fact]
        public async Task RV_LockOrThrottle_Given_LockFailedAndRepeated_Gives_NoLockRetryButCallsElse()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestPostponedException("Already locked"));
            var activity = new ActivityLock<int>(_activityInformationMock, null, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.FromResult(1));
            activity.Else((_, _) => Task.FromResult(2));
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Act
            await activity.LockOrThrottleAsync(CancellationToken.None);

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Else");
            logicExecutor.ExecuteWithReturnValueCounter["Else"].ShouldBe(2);
        }

        [Fact]
        public async Task RV_LockOrThrottle_Given_LockFailedAndNoElse_Gives_Throws()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestPostponedException("Already locked"));
            var activity = new ActivityLock<int>(_activityInformationMock, null, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.FromResult(1));

            // Act
            await Should.ThrowAsync<RequestPostponedException>(activity.LockOrThrottleAsync(CancellationToken.None));

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Else");
        }

        [Fact]
        public async Task RV_LockOrThrottle_Given_LockFailedAndNoElseAndRepeated_Gives_Throws()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            _semaphoreSupportMock.Setup(s => s.RaiseAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestPostponedException("Already locked"));
            var activity = new ActivityLock<int>(_activityInformationMock, null, _semaphoreSupportMock.Object);
            activity.Then((_, _) => Task.FromResult(1));
            await Should.ThrowAsync<RequestPostponedException>(activity.LockOrThrottleAsync(CancellationToken.None));

            // Act
            await Should.ThrowAsync<RequestPostponedException>(activity.LockOrThrottleAsync(CancellationToken.None));

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Lock");
            logicExecutor.ExecuteWithReturnValueCounter["Lock"].ShouldBe(2);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Then");
            logicExecutor.ExecuteWithReturnValueCounter.ShouldNotContainKey("Else");
        }

        #endregion
    }
}

