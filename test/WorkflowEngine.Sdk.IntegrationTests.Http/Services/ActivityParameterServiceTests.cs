using Nexus.Link.Libraries.Core.Error.Logic;
using WorkflowEngine.UnitTests.Abstract.Services;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http.Services
{
    public class ActivityParameterServiceTests : ActivityParameterServiceTestsBase<FulcrumServiceContractException>, IClassFixture<StartupTestFixture>
    {

        public ActivityParameterServiceTests(StartupTestFixture fixture, ITestOutputHelper output)
            : base(fixture.WorkflowRestClients.ActivityParameter)
        {
        }
    }
}
