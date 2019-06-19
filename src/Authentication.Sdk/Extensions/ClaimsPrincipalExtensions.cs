using System;
using System.Security.Claims;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetClientName(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.GetClaimValue(ClaimTypes.Name, principal);
            return result;
        }

        public static string GetOrganization(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.GetClaimValue(ClaimTypeNames.Organization, principal);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = AuthenticationManager.GetClaimValue(ClaimTypeNames.LegacyOrganization, principal);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    Log.LogWarning($"Note! The token uses the legacy attribute {ClaimTypeNames.LegacyOrganization}. Please issue a new token.");
                }
            }
            return result;
        }

        public static string GetEnvironment(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.GetClaimValue(ClaimTypeNames.Environment, principal);
            if (string.IsNullOrWhiteSpace(result))
            {
                result = AuthenticationManager.GetClaimValue(ClaimTypeNames.LegacyEnvironment, principal);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    Log.LogWarning($"Note! The token uses the legacy attribute {ClaimTypeNames.LegacyEnvironment}. Please issue a new token.");
                }
            }
            return result;
        }

        [Obsolete("There is no longer system support for the role 'InternalSystemUser'", true)]
        public static bool IsInternalSystemUser(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.HasRole("InternalSystemUser", principal);
            return result;
        }

        [Obsolete("There is no longer system support for the role 'ExternalSystemUser'", true)]
        public static bool IsExternalSystemUser(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.HasRole("ExternalSystemUser", principal);
            return result;
        }

        /// <summary>
        /// Tells if this principal has the "platform-service" role, indicating the client is a Nexus platform service.
        /// </summary>
        public static bool IsNexusPlatformService(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.HasRole(NexusAuthenticationRoles.PlatformService, principal);
            return result;
        }

        /// <summary>
        /// Tells if this principal has the "api" role, indicating the client is a customer's Nexus api.
        /// </summary>
        public static bool IsNexusApi(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.HasRole(NexusAuthenticationRoles.Api, principal);
            return result;
        }
    }
}
