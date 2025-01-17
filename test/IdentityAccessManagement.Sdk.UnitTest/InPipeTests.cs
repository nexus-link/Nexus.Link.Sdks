using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Pipe;
using UnitTests.Support;
using Xunit;
using Xunit.Abstractions;

namespace IdentityAccessManagement.Sdk.UnitTest
{
    public class InPipeTests : IClassFixture<CustomWebApplicationFactory<TestStartup>>
    {
        private readonly CustomWebApplicationFactory<TestStartup> _factory;

        public InPipeTests(CustomWebApplicationFactory<TestStartup> factory, ITestOutputHelper output)
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(InPipeTests));
            FulcrumApplication.Setup.SynchronousFastLogger = new XUnitFulcrumLogger(output);

            _factory = factory;
        }

        [Fact]
        public async Task Request_Is_Authorized()
        {
            using var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = TestStartup.AuthorizationHeader;
            

            var response = await httpClient.GetAsync("api/home/information");
            var result = await response.Content.ReadAsStringAsync();
            var information = JsonConvert.DeserializeObject<HomeInformation>(result);

            Log.LogInformation($"RESPONSE CODE: {response.StatusCode}, RESULT: {result}");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("Home", information.Data);
        }

        [Fact]
        public async Task Api_Is_Protected()
        {
            using var httpClient = _factory.CreateClient();

            var response = await httpClient.GetAsync("api/home/information");
            var result = await response.Content.ReadAsStringAsync();

            Log.LogInformation($"RESPONSE CODE: {response.StatusCode}, RESULT: {result}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Client_And_User_Principals_Are_Setup_By_Pipe()
        {
            using var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = TestStartup.AuthorizationHeader;
            httpClient.DefaultRequestHeaders.Add(Constants.NexusUserAuthorizationHeaderName, $"Bearer {TestStartup.UserAuthorizationHeader}");

            var response = await httpClient.GetAsync("api/home/information");
            var result = await response.Content.ReadAsStringAsync();
            Log.LogInformation($"RESPONSE CODE: {response.StatusCode}, RESULT: {result}");

            var information = JsonConvert.DeserializeObject<HomeInformation>(result);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(TestStartup.ClientName, information?.ClientPrincipalName);
            Assert.Equal(TestStartup.UserName, information?.UserPrincipalName);
        }

        [Fact]
        public async Task User_Principal_Is_Propagated_RestClient()
        {
            HomeController.Factory = _factory;

            using var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = TestStartup.AuthorizationHeader;
            httpClient.DefaultRequestHeaders.Add(Constants.NexusUserAuthorizationHeaderName, TestStartup.UserAuthorizationHeader);

            var response = await httpClient.GetAsync("api/home/fetchdata");
            var result = await response.Content.ReadAsStringAsync();
            Log.LogInformation($"RESPONSE CODE: {response.StatusCode}, RESULT: {result}");

            var information = JsonConvert.DeserializeObject<HomeInformation>(result);

            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(information?.Data);
            // Expect "fetchdata" endpoint to return User Auth Header content
            Assert.True(information.Data.Contains("Bearer "), information.Data);
        }
    }
}
