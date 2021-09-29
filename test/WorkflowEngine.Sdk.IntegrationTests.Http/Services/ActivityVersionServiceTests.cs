using WorkflowEngine.UnitTests.Abstract.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http.Services
{
    public class ActivityVersionServiceTests : ActivityVersionServiceTestsBase, IClassFixture<StartupTestFixture>
    {

        public ActivityVersionServiceTests(StartupTestFixture fixture, ITestOutputHelper output)
            : base(fixture.WorkflowRestClients.ActivityVersion)
        {
        }
    }
}
