using Nexus.Link.Libraries.Core.Error.Logic;
using WorkflowEngine.UnitTests.Abstract.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http.Services
{
    public class WorkflowInstanceServiceTests : WorkflowInstanceServiceTestsBase<FulcrumServiceContractException>, IClassFixture<StartupTestFixture>
    {

        public WorkflowInstanceServiceTests(StartupTestFixture fixture, ITestOutputHelper output)
            : base(fixture.WorkflowRestClients.WorkflowInstance)
        {
        }
    }
}
