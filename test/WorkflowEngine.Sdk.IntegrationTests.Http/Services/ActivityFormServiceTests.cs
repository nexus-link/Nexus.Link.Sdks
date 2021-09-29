using WorkflowEngine.UnitTests.Abstract.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http.Services
{
    public class ActivityFormServiceTests : ActivityFormServiceTestsBase, IClassFixture<StartupTestFixture>
    {

        public ActivityFormServiceTests(StartupTestFixture fixture, ITestOutputHelper output)
            : base(fixture.WorkflowRestClients.ActivityForm)
        {
        }
    }
}
