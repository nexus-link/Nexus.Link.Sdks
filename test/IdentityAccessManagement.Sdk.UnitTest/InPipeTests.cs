using System.Threading.Tasks;
using IdentityAccessManagement.Sdk.Pipe;
using IdentityAccessManagement.Sdk.Pipe.Outbound;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace IdentityAccessManagement.Sdk.UnitTest
{
    public class InPipeTests : IClassFixture<CustomWebApplicationFactory<TestStartup>>
    {
        private readonly CustomWebApplicationFactory<TestStartup> _factory;
        private readonly ITestOutputHelper _output;

        public InPipeTests(CustomWebApplicationFactory<TestStartup> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact(Skip = "need to find a good way to mock open id connect")]
        public async Task Request_Is_Authorized()
        {
            using var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = TestStartup.AuthorizationHeader;

            var response = await httpClient.GetAsync("api/home/information");
            var result = await response.Content.ReadAsStringAsync();
            var information = JsonConvert.DeserializeObject<HomeInformation>(result);

            _output.WriteLine($"RESPONSE CODE: {response.StatusCode}, RESULT: {result}");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("Home", information.Name);
        }

        [Fact(Skip = "need to find a good way to mock open id connect")]
        public async Task Client_And_User_Principals_Are_Setup_By_Pipe()
        {
            using var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = TestStartup.AuthorizationHeader;
            httpClient.DefaultRequestHeaders.Add(Constants.NexusUserAuthorizationHeaderName, TestStartup.UserAuthorizationHeader);

            var response = await httpClient.GetAsync("api/home/information");
            var result = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"RESPONSE CODE: {response.StatusCode}, RESULT: {result}");

            var information = JsonConvert.DeserializeObject<HomeInformation>(result);
            
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(TestStartup.ClientName, information?.ClientPrincipalName);
            Assert.Equal(TestStartup.UserName, information?.UserPrincipalName);
        }
    }
}
