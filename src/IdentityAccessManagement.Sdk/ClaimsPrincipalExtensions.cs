using System.Linq;
using System.Security.Claims;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace IdentityAccessManagement.Sdk
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetClientName(this ClaimsPrincipal principal)
        {
            var result = principal.GetClaimValue(ClaimTypes.Name);
            return result;
        }

        public static string GetOrganization(this ClaimsPrincipal principal)
        {
            var result = principal.GetClaimValue(ClaimTypeNames.Organization);
            return result;
        }

        public static string GetEnvironment(this ClaimsPrincipal principal)
        {
            var result = principal.GetClaimValue(ClaimTypeNames.Environment);
            return result;
        }


        public static bool HasRole(this ClaimsPrincipal principal, string role)
        {
            var result = principal.IsInRole(role);
            return result;
        }

        public static string GetClaimValue(this ClaimsPrincipal principal, string type)
        {
            var claim = principal.Identities.FirstOrDefault()?.Claims.FirstOrDefault(c => c.Type == type);
            return claim?.Value;
        }

    }
}
