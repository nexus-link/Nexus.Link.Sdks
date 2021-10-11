using WorkflowEngine.UnitTests.Abstract.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http.Services
{
    public class ActivityInstanceServiceTests : ActivityInstanceServiceTestsBase, IClassFixture<StartupTestFixture>
    {

        public ActivityInstanceServiceTests(StartupTestFixture fixture, ITestOutputHelper output)
            : base(fixture.WorkflowRestClients.ActivityInstance)
        {
        }
    }
}
