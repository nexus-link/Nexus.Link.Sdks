using System.Security.Claims;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static void VerifyOrganizationAndEnvironmentOrThrow(this ClaimsPrincipal principal, Tenant tenant)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));

            if (principal.IsSysAdminUser()) return;

            var orgFromToken = principal.GetOrganization()?.ToLower();
            var envFromToken = principal.GetEnvironment()?.ToLower();

            InternalContract.RequireNotNullOrWhiteSpace(orgFromToken, nameof(orgFromToken));
            InternalContract.RequireNotNullOrWhiteSpace(envFromToken, nameof(envFromToken));

            if (orgFromToken != tenant.Organization.ToLower() || envFromToken != tenant.Environment.ToLower())
            {
                throw new FulcrumUnauthorizedException($"Authorized user is not allowed access to '{tenant.Organization}', '{tenant.Environment}'.");
            }
        }

        public static string GetClientName(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.GetClaimValue(ClaimTypes.Name, principal);
            return result;
        }

        public static string GetOrganization(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.GetClaimValue(ClaimTypeNames.Organization, principal);
            return result;
        }

        public static string GetEnvironment(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.GetClaimValue(ClaimTypeNames.Environment, principal);
            return result;
        }

        public static bool IsInternalSystemUser(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.HasRole(AuthenticationRoleEnum.InternalSystemUser, principal);
            return result;
        }

        public static bool IsExternalSystemUser(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.HasRole(AuthenticationRoleEnum.ExternalSystemUser, principal);
            return result;
        }

        public static bool IsOrganizationAdmin(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.HasRole(AuthenticationRoleEnum.OrganizationAdmin, principal);
            return result;
        }

        public static bool IsSysAdminUser(this ClaimsPrincipal principal)
        {
            var result = AuthenticationManager.HasRole(AuthenticationRoleEnum.SysAdminUser, principal);
            return result;
        }
    }
}
