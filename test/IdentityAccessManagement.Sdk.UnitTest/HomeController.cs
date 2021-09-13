using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Libraries.Web.Pipe.Outbound;

namespace IdentityAccessManagement.Sdk.UnitTest
{
    [Authorize(Roles = "consumer")]
    [Route("api/home")]
    public class HomeController : Controller
    {
        public static CustomWebApplicationFactory<TestStartup> Factory { get; set; }

        [HttpGet]
        [Route("information")]
        public HomeInformation GetInformation()
        {
            return new HomeInformation
            {
                Data = "Home",
                ClientPrincipalName = (FulcrumApplication.Context.ClientPrincipal as ClaimsPrincipal)?.GetClientName(),
                UserPrincipalName = (FulcrumApplication.Context.UserPrincipal as ClaimsPrincipal)?.GetClientName()
            };
        }

        [HttpGet]
        [Route("fetchdata")]
        public async Task<HomeInformation> FetchData()
        {
            FulcrumAssert.IsNotNull(Factory);

            // TODO: Use RestClient?
            using var httpClient = Factory.CreateDefaultClient(new AddUserAuthorization());

            var data = await httpClient.GetStringAsync("api/home/data");

            return new HomeInformation
            {
                Data = data,
                ClientPrincipalName = (FulcrumApplication.Context.ClientPrincipal as ClaimsPrincipal)?.GetClientName(),
                UserPrincipalName = (FulcrumApplication.Context.UserPrincipal as ClaimsPrincipal)?.GetClientName()
            };
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("data")]
        public string Data()
        {
            return Request.Headers[Constants.NexusUserAuthorizationHeaderName];
        }
    }
}
