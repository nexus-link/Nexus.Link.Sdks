using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivityActionLockTests : ActivityTestsBase
{
    private readonly Mock<ISemaphoreSupport> _semaphoreSupportMock;

    public ActivityActionLockTests() : base(nameof(ActivityActionLockTests))
    {
        _semaphoreSupportMock = new Mock<ISemaphoreSupport>();
        _semaphoreSupportMock.SetupGet(s => s.IsThrottle).Returns(false);
        _semaphoreSupportMock.SetupGet(s => s.Limit).Returns(1);
    }

    #region No return value
    [Fact]
    public async Task Execute_Given_Success_Gives_LockRaisedAndLowered()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction(_activityInformationMock, (_, _) => Task.CompletedTask)
        {
            LockSemaphoreSupport = _semaphoreSupportMock.Object
        };


        // Act
        await activity.ActionAsync();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task Execute_Given_Success_Gives_ThrottleRaisedAndLowered()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction(_activityInformationMock, (_, _) => Task.CompletedTask)
        {
            ThrottleSemaphoreSupport = _semaphoreSupportMock.Object
        };


        // Act
        await activity.ActionAsync();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
    [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
    [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
    public async Task Execute_Given_ActivityFailed_Gives_LockRaisedAndLowered(ActivityExceptionCategoryEnum category)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction(_activityInformationMock, (_, _) => throw new ActivityFailedException(category, "technical", "friendly"))
        {
            LockSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync()
            .ShouldThrowAsync<ActivityFailedException>();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
    [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
    [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
    public async Task Execute_Given_ActivityFailed_Gives_ThrottleRaisedAndLowered(ActivityExceptionCategoryEnum category)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction(_activityInformationMock, (_, _) => throw new ActivityFailedException(category, "technical", "friendly"))
        {
            ThrottleSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync()
            .ShouldThrowAsync<ActivityFailedException>();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_Given_Postponed_Gives_LockRaisedButNotLowered()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction(_activityInformationMock, (_, _) => throw new ActivityPostponedException(null))
        {
            LockSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_Given_Postponed_Gives_ThrottleRaisedButNotLowered()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction(_activityInformationMock, (_, _) => throw new ActivityPostponedException(null))
        {
            ThrottleSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion

    #region Return value
    [Fact]
    public async Task RV_Execute_Given_Success_LockGives_RaisedAndLowered()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => Task.FromResult(1))
        {
            LockSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task RV_Execute_Given_Success_Gives_ThrottleRaisedAndLowered()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => Task.FromResult(1))
        {
            ThrottleSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
    [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
    [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
    public async Task RV_Execute_Given_ActivityFailed_Gives_LockRaisedAndLowered(ActivityExceptionCategoryEnum category)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => throw new ActivityFailedException(category, "technical", "friendly"))
        {
            LockSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync()
            .ShouldThrowAsync<ActivityFailedException>();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
    [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
    [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
    [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
    public async Task RV_Execute_Given_ActivityFailed_Gives_ThrottleRaisedAndLowered(ActivityExceptionCategoryEnum category)
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => throw new ActivityFailedException(category, "technical", "friendly"))
        {
            ThrottleSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync()
            .ShouldThrowAsync<ActivityFailedException>();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RV_Execute_Given_Postponed_Gives_LockRaisedButNotLowered()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => throw new ActivityPostponedException(null))
        {
            LockSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RV_Execute_Given_Postponed_Gives_ThrottleRaisedButNotLowered()
    {
        // Arrange
        var logicExecutor = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutor;
        var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => throw new ActivityPostponedException(null))
        {
            ThrottleSemaphoreSupport = _semaphoreSupportMock.Object
        };

        // Act
        await activity.ActionAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        _semaphoreSupportMock.Verify(e => e.RaiseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _semaphoreSupportMock.Verify(e => e.LowerAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}