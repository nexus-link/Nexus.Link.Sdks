using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities
{
    public class ActivityParallelTests : ActivityTestsBase
    {
        public ActivityParallelTests() :base(nameof(ActivityParallelTests))
        {
        }

        [Fact]
        public async Task Execute_Given_Normal_Gives_ActivityExecutorActivated()
        {
            // Arrange
            var activity = new ActivityParallel(_activityInformationMock);

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(e => e.ExecuteWithReturnValueAsync(activity.ParallelAsync, It.IsAny<ActivityDefaultValueMethodAsync<JobResults>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Parallel_Given_NoJobs_Gives_NoCall()
        {
            // Arrange
            var logicExecutorMock = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutorMock;
            var activity = new ActivityParallel(_activityInformationMock);

            // Act
            await activity.ParallelAsync();

            // Assert
            logicExecutorMock.ExecuteWithReturnValueCounter.Count.ShouldBe(0);
            logicExecutorMock.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task Parallel_Given_JobsWithoutResult_Gives_Call(int jobs)
        {
            // Arrange
            var logicExecutorMock = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutorMock;
            var activity = new ActivityParallel(_activityInformationMock);
            for (int i = 0; i < jobs; i++)
            {
                int jobNumber = i + 1;
                activity.AddJob(jobNumber, (_, _) => Task.CompletedTask);
            }

            // Act
            var results = await activity.ParallelAsync();

            // Assert
            results.ShouldNotBeNull();
            logicExecutorMock.ExecuteWithReturnValueCounter.Count.ShouldBe(0);
            logicExecutorMock.ExecuteWithoutReturnValueCounter.Count.ShouldBe(jobs);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task Parallel_Given_JobsWithResult_Gives_Call(int jobs)
        {
            // Arrange
            var logicExecutorMock = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutorMock;
            var activity = new ActivityParallel(_activityInformationMock);
            for (var i = 0; i < jobs; i++)
            {
                int jobNumber = i + 1;
                activity.AddJob(jobNumber, (_, _) => Task.FromResult(jobNumber));
            }

            // Act
            var results = await activity.ParallelAsync();

            // Assert
            results.ShouldNotBeNull();
            for (var i = 0; i < jobs; i++)
            {
                results.Get<int>(i+1).ShouldBe(i+1);
            }
            logicExecutorMock.ExecuteWithReturnValueCounter.Count.ShouldBe(jobs);
            logicExecutorMock.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
        }

        [Fact]
        public void AddJob_Given_Doublet_Gives_Exception()
        {
            // Arrange
            var logicExecutorMock = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutorMock;
            var activity = new ActivityParallel(_activityInformationMock);
            activity.AddJob(1, (_, _) => Task.CompletedTask);

            // Act
            Should.Throw<FulcrumContractException>(() => activity.AddJob(1, (_, _) => Task.CompletedTask));
        }

        [Fact]
        public async Task Parallel_Given_TwoJobs_Gives_BothStartedSimultaneously()
        {
            // Arrange
            var logicExecutorMock = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutorMock;
            var activity = new ActivityParallel(_activityInformationMock);
            var started1 = new ManualResetEventSlim(false);
            var started2 = new ManualResetEventSlim(false);
            activity.AddJob(1, async (_, ct) =>
            {
                started1.Set();
                while (!started2.IsSet) await Task.Delay(1, ct);
            });
            activity.AddJob(2, async (_, ct) =>
            {
                started2.Set();
                while (!started1.IsSet) await Task.Delay(1, ct);
            });

            // Act
            await activity.ParallelAsync();
        }

        [Fact]
        public async Task ExecuteJobsAsync_Given_JobWithReturnValue_ReturnsExpectedValue()
        {
            // Arrange
            var logicExecutorMock = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutorMock;
            var activity = new ActivityParallel(_activityInformationMock);
            const int expectedResult = 10;
            activity.AddJob(1, (_, _) => Task.FromResult(expectedResult));

            // Act
            var results = await activity.ParallelAsync();

            // Assert
            var result = results.Get<int>(1);
            result.ShouldBe(expectedResult);
        }
    }
}

