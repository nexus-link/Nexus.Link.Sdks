using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic
{
    public class ActivityParallelTests
    {

        public ActivityParallelTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityParallelTests));
        }

        [Fact]
        public async Task Execute_Given_MethodReturns_Gives_Success()
        {
            // Arrange
            var activityInformation = new ActivityInformationMock();
            var activity = new ActivityParallel(activityInformation);
            activity.AddJob(1, (a, ct) => Task.CompletedTask);

            // Act
            await activity.ExecuteAsync();
        }

        [Fact]
        public async Task Execute_Given_TwoMethods_Gives_BothStartedSimultanously()
        {
            // Arrange
            var activityInformation = new ActivityInformationMock();
            var activity = new ActivityParallel(activityInformation);
            var started1 = new ManualResetEventSlim(false);
            var started2 = new ManualResetEventSlim(false);
            activity.AddJob(1, async (a, ct) =>
            {
                started1.Set();
                while (!started2.IsSet) await Task.Delay(1);
            });
            activity.AddJob(2, async (a, ct) =>
            {
                started2.Set();
                while (!started1.IsSet) await Task.Delay(1);
            });

            // Act
            await activity.ExecuteAsync();
        }
    }
}

