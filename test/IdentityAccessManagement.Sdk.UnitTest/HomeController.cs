using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Application;

namespace IdentityAccessManagement.Sdk.UnitTest
{
    [Authorize(Roles = "consumer")]
    [Route("api/home")]
    public class HomeController : Controller
    {

        [HttpGet]
        [Route("information")]
        public HomeInformation GetInformation()
        {
            return new HomeInformation
            {
                Name = "Home",
                ClientPrincipalName = (FulcrumApplication.Context.ClientPrincipal as ClaimsPrincipal)?.GetClientName(),
                UserPrincipalName = (FulcrumApplication.Context.UserPrincipal as ClaimsPrincipal)?.GetClientName()
            };
        }
    }
}
