using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;
#pragma warning disable CS0618

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivitySemaphoreTests : ActivityTestsBase
{
    private readonly Mock<IWorkflowSemaphoreService> _semaphoreServiceMock;

    public ActivitySemaphoreTests() :base(nameof(ActivitySemaphoreTests))
    {
        _semaphoreServiceMock = new Mock<IWorkflowSemaphoreService>();
        _workflowInformationMock.SemaphoreService = _semaphoreServiceMock.Object;
        _semaphoreServiceMock.Setup(s =>
                s.RaiseAsync(It.IsAny<WorkflowSemaphoreCreate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkflowSemaphoreCreate wsc, CancellationToken ct) => wsc.WorkflowInstanceId);
    }

    [Fact]
    public async Task Raise_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivitySemaphore(_activityInformationMock, Guid.NewGuid().ToString());

        // Act
        await activity.RaiseAsync(TimeSpan.FromHours(1));

        // Assert
        _activityExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Lower_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivitySemaphore(_activityInformationMock, Guid.NewGuid().ToString());

        // Act
        await activity.LowerAsync();

        // Assert
        _activityExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InternalRaiseWithLimit_Given_Normal_Gives_Raise()
    {
        // Arrange
        var logicExecutorMock = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutorMock;
        var activity = new ActivitySemaphore(_activityInformationMock, Guid.NewGuid().ToString());

        // Act
        await activity.InternalRaiseWithLimitAsync(1, TimeSpan.FromHours(1));

        // Assert
        logicExecutorMock.ExecuteWithReturnValueCounter.Count.ShouldBe(0);
        logicExecutorMock.ExecuteWithoutReturnValueCounter.Count.ShouldBe(1);
        logicExecutorMock.ExecuteWithoutReturnValueCounter.ShouldContainKey("Raise");
        logicExecutorMock.ExecuteWithoutReturnValueCounter["Raise"].ShouldBe(1);
    }

    [Fact]
    public async Task InternalLower_Given_Normal_Gives_Lower()
    {
        // Arrange
        var logicExecutorMock = new LogicExecutorMock();
        _workflowInformationMock.LogicExecutor = logicExecutorMock;
        var resourceIdentifier = Guid.NewGuid().ToString();
        var activity1 = new ActivitySemaphore(_activityInformationMock, resourceIdentifier);
        await activity1.InternalRaiseWithLimitAsync(1, TimeSpan.FromHours(1));
        var activity2 = new ActivitySemaphore(_activityInformationMock, resourceIdentifier);

        // Act
        await activity2.InternalLowerAsync();

        // Assert
        logicExecutorMock.ExecuteWithReturnValueCounter.Count.ShouldBe(0);
        logicExecutorMock.ExecuteWithoutReturnValueCounter.Count.ShouldBe(2);
        logicExecutorMock.ExecuteWithoutReturnValueCounter.ShouldContainKey("Raise");
        logicExecutorMock.ExecuteWithoutReturnValueCounter.Count.ShouldBe(2);
        logicExecutorMock.ExecuteWithoutReturnValueCounter.ShouldContainKey("Lower");
        logicExecutorMock.ExecuteWithoutReturnValueCounter["Lower"].ShouldBe(1);
    }
}