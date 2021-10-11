using Nexus.Link.Libraries.Core.Error.Logic;
using WorkflowEngine.UnitTests.Abstract.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http.Services
{
    public class WorkflowParameterServiceTests : WorkflowParameterServiceTestsBase<FulcrumServiceContractException>, IClassFixture<StartupTestFixture>
    {

        public WorkflowParameterServiceTests(StartupTestFixture fixture, ITestOutputHelper output)
            : base(fixture.WorkflowRestClients.WorkflowParameter)
        {
        }
    }
}
