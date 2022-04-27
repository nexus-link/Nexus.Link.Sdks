using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities
{
    public class ActivityParallelTests
    {
        private readonly Mock<IActivityExecutor> _activityExecutorMock;
        private readonly ActivityInformationMock _activityInformationMock;
        public ActivityParallelTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityParallelTests));
            _activityExecutorMock = new Mock<IActivityExecutor>();
            var workflowInformationMock = new WorkflowInformationMock(_activityExecutorMock.Object);
            _activityInformationMock = new ActivityInformationMock(workflowInformationMock);
        }

        [Fact]
        public async Task Execute_Given_NoJobs_Gives_Call()
        {
            // Arrange
            var activity = new ActivityParallel(_activityInformationMock);

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(
                ae => ae.ExecuteWithReturnValueAsync(It.IsAny<ActivityMethodAsync<JobResults>>(), null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Execute_Given_OneJob_Gives_Call()
        {
            // Arrange
            var activity = new ActivityParallel(_activityInformationMock);
            activity.AddJob(1, (a, ct) => Task.CompletedTask);

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(
                ae => ae.ExecuteWithReturnValueAsync(It.IsAny<ActivityMethodAsync<JobResults>>(), null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void AddJob_Given_Doublet_Gives_Exception()
        {
            // Arrange
            var activity = new ActivityParallel(_activityInformationMock);
            activity.AddJob(1, (a, ct) => Task.CompletedTask);

            // Act
            Shouldly.Should.Throw<FulcrumContractException>(() => activity.AddJob(1, (a, ct) => Task.CompletedTask));
        }

        [Fact]
        public async Task Execute_Given_TwoMethods_Gives_BothStartedSimultaneously()
        {
            // Arrange
            var activity = new ActivityParallel(_activityInformationMock);
            var started1 = new ManualResetEventSlim(false);
            var started2 = new ManualResetEventSlim(false);
            activity.AddJob(1, async (a, ct) =>
            {
                started1.Set();
                while (!started2.IsSet) await Task.Delay(1, ct);
            });
            activity.AddJob(2, async (a, ct) =>
            {
                started2.Set();
                while (!started1.IsSet) await Task.Delay(1, ct);
            });

            // Act
            await activity.ExecuteJobsAsync();
        }
    }
}

