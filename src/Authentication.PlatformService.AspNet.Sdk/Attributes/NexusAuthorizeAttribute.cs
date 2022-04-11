using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Platform.Authentication;
#if NETCOREAPP
using Microsoft.AspNetCore.Authorization;
#else
using System.Web.Http;
using System.Web.Http.Controllers;
using Nexus.Link.Libraries.Core.Logging;
#endif

namespace Nexus.Link.Authentication.PlatformService.AspNet.Sdk.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Authorization used by Nexus services.
    /// </summary>
    /// <remarks>Not intended for use in customer's apis and adapters.</remarks>
    public class NexusAuthorizeAttribute : AuthorizeAttribute
    {
        public NexusAuthorizeAttribute(params string[] roles)
        {
            Roles = RestrictTo(roles);
        }

#if NETCOREAPP
#else
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var authorized = base.IsAuthorized(actionContext);
            if (!authorized)
            {
                var principal = actionContext.ControllerContext.RequestContext.Principal;
                Log.LogInformation($"Principal '{principal?.Identity?.Name}' does not have a required role ({Roles})");
            }
            return authorized;
        }
#endif

        private static string RestrictTo(params string[] roles)
        {
            // "platform-service" always has access
            // We depend on TokenValidationHandler to authenticate the calling client
            var result = new List<string> { NexusAuthenticationRoles.PlatformService };
            if (roles != null)
            {
                result.AddRange(roles);
            }
            return string.Join(",", result);
        }

    }
}
