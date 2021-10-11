using WorkflowEngine.UnitTests.Abstract.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http.Services
{
    public class TransitionServiceTests : TransitionServiceTestsBase, IClassFixture<StartupTestFixture>
    {

        public TransitionServiceTests(StartupTestFixture fixture, ITestOutputHelper output)
            : base(fixture.WorkflowRestClients.Transition)
        {
        }
    }
}
