using System.Net.Http;
using System.Net.Http.Headers;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.RestClients;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.IntegrationTests.Http
{
    public class HttpToApiTestBase : IClassFixture<StartupTestFixture>
    {
        public StartupTestFixture Fixture { get; }
        public ITestOutputHelper Output { get; }
        protected readonly HttpClient ApiClient;
        protected IWorkflowCapability WorkflowCapability { get; }

        protected HttpToApiTestBase(StartupTestFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
            ApiClient = fixture.CreateClient();
            IHttpSender httpSender = new HttpSender(null)
            {
                HttpClient = new HttpClientWrapper(ApiClient)
            };
            WorkflowCapability = new WorkflowRestClients(httpSender.CreateHttpSender("WorkflowEngine"));
            ApiClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}