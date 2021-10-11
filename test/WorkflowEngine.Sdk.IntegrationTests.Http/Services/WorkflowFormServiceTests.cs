using WorkflowEngine.UnitTests.Abstract.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http.Services
{
    public class WorkflowFormServiceTests : WorkflowFormServiceTestsBase, IClassFixture<StartupTestFixture>
    {

        public WorkflowFormServiceTests(StartupTestFixture fixture, ITestOutputHelper output)
            : base(fixture.WorkflowRestClients.WorkflowForm)
        {
        }
    }
}
