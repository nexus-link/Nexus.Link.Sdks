using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic
{
    public class ActivitySleepTests
    {

        public ActivitySleepTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivitySleepTests));
        }

        [Fact]
        public async Task Execute_Given_NormalSleep_Gives_ExceptionWithSleepTime()
        {
            // Arrange
            var activityInformation = new ActivityInformationMock();
            var timeSpan = TimeSpan.FromMilliseconds(10);
            var activity = new ActivitySleep(activityInformation, timeSpan);

            // Act
            var exception = await activity.ExecuteAsync()
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();

            // Assert
            exception.InnerException.ShouldNotBeNull();
            var innerException = exception.InnerException as RequestPostponedException;
            innerException.ShouldNotBeNull();
            innerException.TryAgainAfterMinimumTimeSpan.ShouldNotBeNull();
            innerException.TryAgainAfterMinimumTimeSpan.Value.ShouldBeGreaterThan(TimeSpan.Zero);
            innerException.TryAgainAfterMinimumTimeSpan.Value.ShouldBeLessThanOrEqualTo(timeSpan);
        }

        [Fact]
        public async Task Execute_Given_Wait_Gives_NoException()
        {
            // Arrange
            var activityInformation = new ActivityInformationMock();
            var timeSpan = TimeSpan.FromMilliseconds(10);
            var activity = new ActivitySleep(activityInformation, timeSpan);
            var exception = await activity.ExecuteAsync()
                .ShouldThrowAsync<WorkflowImplementationShouldNotCatchThisException>();
            exception.InnerException.ShouldNotBeNull();
            var innerException = exception.InnerException as RequestPostponedException;
            innerException.ShouldNotBeNull();
            innerException.TryAgainAfterMinimumTimeSpan.ShouldNotBeNull();
            if (innerException.TryAgainAfterMinimumTimeSpan.Value > TimeSpan.Zero)
            {
                await Task.Delay(innerException.TryAgainAfterMinimumTimeSpan.Value);
            }

            // Act
            await activity.ExecuteAsync();
        }
    }
}

