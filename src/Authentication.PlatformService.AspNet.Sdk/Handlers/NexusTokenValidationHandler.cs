using System.Security.Claims;
using System.Threading.Tasks;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Authentication.Sdk.Extensions;
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

#if NETCOREAPP
        /// <inheritdoc />
        public NexusTokenValidationHandler(RequestDelegate next, string fundamentalsServiceBaseUrl) : base(next, fundamentalsServiceBaseUrl, AuthenticationManager.NexusIssuer)
        {
        }
#else
        public NexusTokenValidationHandler(string fundamentalsServiceBaseUrl) : base(fundamentalsServiceBaseUrl, AuthenticationManager.NexusIssuer)
        {
        }
#endif

        protected override async Task<string> FetchPublicKeyXmlAsync(Tenant tenant)
        {
            var publicKeyXml = await AuthenticationManager.GetNexusPublicKeyXmlAsync(tenant, FundamentalsServiceBaseUrl);
            return publicKeyXml;
        }

        protected override bool ClaimHasCorrectTenant(ClaimsPrincipal claimsPrincipal, Tenant tenant)
        {
            if (claimsPrincipal == null)
            {
                Log.LogVerbose("Claims principal was null.");
                return false;
            }

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

    }
}
