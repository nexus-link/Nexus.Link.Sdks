using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAccessManagement.Sdk.Authorization
{
    /// <summary>
    /// Use in Controllers to require a certain scope value for the current the <see cref="ClaimsPrincipal"/> for access.
    /// </summary>
    public class ScopeRequirementAttribute : TypeFilterAttribute
    {
        public ScopeRequirementAttribute(string claimValue) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { new Claim(JwtClaimTypes.Scope, claimValue) };
        }
    }
}
