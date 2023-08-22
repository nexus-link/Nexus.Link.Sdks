using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities;

public class ActivitySleepTests : ActivityTestsBase
{
    public ActivitySleepTests() : base(nameof(ActivitySleepTests))
    {
    }

    [Fact]
    public async Task Execute_Given_Normal_Gives_ActivityExecutorActivated()
    {
        // Arrange
        var activity = new ActivitySleep(_activityInformationMock, TimeSpan.FromSeconds(1));

        // Act
        await activity.ExecuteAsync();

        // Assert
        _activityExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(activity.SleepAsync, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sleep_Given_PositiveTimeSpan_Gives_ExceptionWithSleepTime()
    {
        // Arrange
        var timeSpan = TimeSpan.FromSeconds(10);
        var activity = new ActivitySleep(_activityInformationMock, timeSpan);
        var now = DateTimeOffset.UtcNow;

        // Act
        var exception = await activity.SleepAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        exception.ShouldNotBeNull();
        exception.TryAgainAfterMinimumTimeSpan.ShouldNotBeNull();
        exception.TryAgainAfterMinimumTimeSpan.Value.ShouldBeGreaterThan(timeSpan.Subtract(TimeSpan.FromSeconds(1)));
        exception.TryAgainAfterMinimumTimeSpan.Value.ShouldBeLessThanOrEqualTo(timeSpan.Add(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Sleep_Given_FutureTime_Gives_ExceptionWithSleepTime()
    {
        // Arrange
        var timeSpan = TimeSpan.FromSeconds(10);
        var activity = new ActivitySleep(_activityInformationMock, DateTimeOffset.UtcNow.Add(timeSpan));
        var now = DateTimeOffset.UtcNow;

        // Act
        var exception = await activity.SleepAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        exception.ShouldNotBeNull();
        exception.TryAgainAfterMinimumTimeSpan.ShouldNotBeNull();
        exception.TryAgainAfterMinimumTimeSpan.Value.ShouldBeGreaterThan(timeSpan.Subtract(TimeSpan.FromSeconds(1)));
        exception.TryAgainAfterMinimumTimeSpan.Value.ShouldBeLessThanOrEqualTo(timeSpan.Add(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task Execute_Given_OldTime_Gives_NoException()
    {
        // Arrange
        var activity = new ActivitySleep(_activityInformationMock, DateTimeOffset.UtcNow);

        // Act && Assert
        await activity.SleepAsync().ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Execute_Given_ZeroTimeSpan_Gives_NoException()
    {
        // Arrange
        var activity = new ActivitySleep(_activityInformationMock, TimeSpan.Zero);

        // Act && Assert
        await activity.SleepAsync().ShouldNotThrowAsync();
    }
}