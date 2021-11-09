using Nexus.Link.Libraries.Core.Error.Logic;
using WorkflowEngine.UnitTests.Abstract.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http.Services
{
    public class WorkflowVersionServiceTests : WorkflowVersionServiceTestsBase<FulcrumServiceContractException>, IClassFixture<StartupTestFixture>
    {

        public WorkflowVersionServiceTests(StartupTestFixture fixture, ITestOutputHelper output)
            : base(fixture.WorkflowRestClients.WorkflowVersion)
        {
        }
    }
}
