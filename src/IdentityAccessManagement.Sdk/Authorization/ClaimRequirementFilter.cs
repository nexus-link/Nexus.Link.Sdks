using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nexus.Link.Libraries.Core.Logging;

namespace IdentityAccessManagement.Sdk.Authorization
{
    /// <summary>
    /// Use in conjunction with a <see cref="TypeFilterAttribute"/>
    /// such as <see cref="ScopeRequirementAttribute"/> to require a certain claim value
    /// for the current the <see cref="ClaimsPrincipal"/> for access.
    /// </summary>
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly Claim _claim;

        public ClaimRequirementFilter(Claim claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User?.Identity.IsAuthenticated == false) return;

            var hasClaim = context.HttpContext.User?.Claims.Any(c => c.Type == _claim.Type && c.Value == _claim.Value) ?? false;
            if (!hasClaim)
            {
                Log.LogInformation($"Authorization failed for principal '{context.HttpContext.User?.Identity?.Name}'." +
                                   $" Required scope '{_claim.Value}' was missing.");

                context.Result = new ForbidResult();
            }
        }
    }
}