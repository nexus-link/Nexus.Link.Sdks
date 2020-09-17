using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Authentication.Sdk.Extensions;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#else

#endif

namespace Nexus.Link.Authentication.PlatformService.AspNet.Sdk.Handlers
{

    public class NexusTokenValidationHandler : TokenValidationHandlerBase
    {
        private readonly string _fundamentalsServiceBaseUrl;

#if NETCOREAPP
        /// <inheritdoc />
        public NexusTokenValidationHandler(RequestDelegate next, string fundamentalsServiceBaseUrl = null) : base(next, AuthenticationManager.NexusIssuer)
        {
            _fundamentalsServiceBaseUrl = fundamentalsServiceBaseUrl;
        }
#else
        public NexusTokenValidationHandler(string fundamentalsServiceBaseUrl = null) : base(AuthenticationManager.NexusIssuer)
        {
            _fundamentalsServiceBaseUrl = fundamentalsServiceBaseUrl;
        }
#endif

        protected override bool ClaimHasCorrectTenant(ClaimsPrincipal claimsPrincipal, Tenant tenant)
        {
            InternalContract.RequireNotNull(claimsPrincipal, nameof(claimsPrincipal));

            var orgFromToken = claimsPrincipal.GetOrganization()?.ToLower();
            var envFromToken = claimsPrincipal.GetEnvironment()?.ToLower();
            var sameTenant = orgFromToken == tenant.Organization.ToLower() && envFromToken == tenant.Environment.ToLower();
            if (sameTenant) return true;

            // A client with "platform-service" role in the "fulcrum" organization can call other Nexus services on behalf of customer tenants.
            if (claimsPrincipal.IsNexusPlatformService() && orgFromToken == "fulcrum")
            {
                // TODO: Should we check that FulcrumApplication.Setup.Tenant.Environment == envFromToken?
                return true;
            }

            var message = $"Claims principal had tenant {orgFromToken}/{envFromToken}. Expected tenant: {tenant}.";
            Log.LogInformation(message);
            return false;
        }

        protected override async Task<RsaSecurityKey> GetPublicKeyAsync(Tenant tenant)
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(_fundamentalsServiceBaseUrl, null, "We need a url to Fundamentals");

            var publicKeyXml = await NexusAuthenticationManager.GetPublicKeyXmlAsync(tenant, _fundamentalsServiceBaseUrl);
            return publicKeyXml == null ? null : AuthenticationManager.CreateRsaSecurityKeyFromXmlString(publicKeyXml);
        }
    }
}
