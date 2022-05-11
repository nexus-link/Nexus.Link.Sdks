using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities
{
    public class ActivitySleepTests
    {
        private readonly Mock<IActivityExecutor> _activityExecutorMock;
        private readonly ActivityInformationMock _activityInformationMock;
        public ActivitySleepTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivitySleepTests));
            _activityExecutorMock = new Mock<IActivityExecutor>();
            var workflowInformationMock = new WorkflowInformationMock(_activityExecutorMock.Object);
            _activityInformationMock = new ActivityInformationMock(workflowInformationMock);
        }

        [Fact]
        public async Task Sleep_Given_NormalSleep_Gives_ExceptionWithSleepTime()
        {
            // Arrange
            var timeSpan = TimeSpan.FromSeconds(10);
            var activity = new ActivitySleep(_activityInformationMock, timeSpan);
            var now = DateTimeOffset.UtcNow;

            // Act
            var exception = await activity.SleepAsync(now)
                .ShouldThrowAsync<RequestPostponedException>();

            // Assert
            exception.ShouldNotBeNull();
            exception.TryAgainAfterMinimumTimeSpan.ShouldNotBeNull();
            exception.TryAgainAfterMinimumTimeSpan.Value.ShouldBeGreaterThan(timeSpan.Subtract(TimeSpan.FromSeconds(1)));
            exception.TryAgainAfterMinimumTimeSpan.Value.ShouldBeLessThanOrEqualTo(timeSpan.Add(TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public async Task Execute_Given_Timeout_Gives_NoException()
        {
            // Arrange
            var timeSpan = TimeSpan.FromSeconds(10);
            var activity = new ActivitySleep(_activityInformationMock, timeSpan);
            _activityExecutorMock.Setup(ae => ae.ExecuteWithReturnValueAsync(It.IsAny<InternalActivityMethodAsync<DateTimeOffset>>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DateTimeOffset.UtcNow.Subtract(timeSpan));

            // Act
            await activity.ExecuteAsync().ShouldNotThrowAsync();

            // Act
            await activity.ExecuteAsync();
        }
    }
}

