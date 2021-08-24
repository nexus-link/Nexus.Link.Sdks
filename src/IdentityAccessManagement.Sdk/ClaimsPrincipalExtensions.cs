using System;
using System.Security.Claims;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace IdentityAccessManagement.Sdk
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetClientName(this ClaimsPrincipal principal)
        {
            var result = IamAuthenticationManager.GetClaimValue(ClaimTypes.Name, principal);
            return result;
        }

        public static string GetOrganization(this ClaimsPrincipal principal)
        {
            var result = IamAuthenticationManager.GetClaimValue(ClaimTypeNames.Organization, principal);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = IamAuthenticationManager.GetClaimValue(ClaimTypeNames.LegacyOrganization, principal);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    Log.LogVerbose($"Note! The token uses the legacy attribute {ClaimTypeNames.LegacyOrganization}. Please issue a new token.");
                }
            }
            return result;
        }

        public static string GetEnvironment(this ClaimsPrincipal principal)
        {
            var result = IamAuthenticationManager.GetClaimValue(ClaimTypeNames.Environment, principal);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = IamAuthenticationManager.GetClaimValue(ClaimTypeNames.LegacyEnvironment, principal);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    Log.LogVerbose($"Note! The token uses the legacy attribute {ClaimTypeNames.LegacyEnvironment}. Please issue a new token.");
                }
            }
            return result;
        }

        /// <summary>
        /// Tells if this principal has the "api" role, indicating the client is a customer's Nexus api.
        /// </summary>
        public static bool IsNexusApi(this ClaimsPrincipal principal)
        {
            var result = IamAuthenticationManager.HasRole(NexusAuthenticationRoles.Api, principal);
            return result;
        }
    }
}
