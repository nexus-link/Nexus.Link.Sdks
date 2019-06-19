using System.Security.Claims;
using System.Threading.Tasks;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Authentication.Sdk.Extensions;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
#else

#endif

namespace Nexus.Link.Authentication.AspNet.Sdk.Handlers
{

    public class TokenValidationHandler : TokenValidationHandlerBase
    {

#if NETCOREAPP
        /// <inheritdoc />
        public TokenValidationHandler(RequestDelegate next, string fundamentalsServiceBaseUrl)
            : this(next, fundamentalsServiceBaseUrl, false)
        {
        }
        public TokenValidationHandler(RequestDelegate next, string fundamentalsServiceBaseUrl, bool legacyIssuer)
            : base(next, fundamentalsServiceBaseUrl, legacyIssuer ? AuthenticationManager.LegacyIssuer : AuthenticationManager.AuthServiceIssuer)
        {
        }
#else
        public TokenValidationHandler(string fundamentalsServiceBaseUrl) : this(fundamentalsServiceBaseUrl, false)
        {
        }
        public TokenValidationHandler(string fundamentalsServiceBaseUrl, bool legacyIssuer)
            : base(fundamentalsServiceBaseUrl,  legacyIssuer ? AuthenticationManager.LegacyIssuer : AuthenticationManager.AuthServiceIssuer)
        {
        }
#endif

        protected override async Task<string> FetchPublicKeyXmlAsync(Tenant tenant)
        {
            var publicKeyXml = await AuthenticationManager.GetPublicKeyXmlAsync(tenant, FundamentalsServiceBaseUrl);
            return publicKeyXml;
        }

        protected override bool ClaimHasCorrectTenant(ClaimsPrincipal claimsPrincipal, Tenant tenant)
        {
            if (claimsPrincipal == null)
            {
                Log.LogVerbose("Claims principal was null.");
                return false;
            }

            // See https://www.lucidchart.com/publicSegments/view/045c8f44-0466-4ca4-b718-fe1b73843566/image.png

            var orgFromToken = claimsPrincipal.GetOrganization()?.ToLower();
            var envFromToken = claimsPrincipal.GetEnvironment()?.ToLower();
            var sameTenant = orgFromToken == tenant.Organization.ToLower() && envFromToken == tenant.Environment.ToLower();
            if (sameTenant) return true;

            var message = $"Claims principal had tenant {orgFromToken}/{envFromToken}. Expected tenant: {tenant}.";
            Log.LogInformation(message);
            return false;
        }
    }
#if NETCOREAPP
    public static class TokenValidationHandlerExtension
    {
        public static IApplicationBuilder UseNexusTokenValidationHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenValidationHandler>();
        }
    }
#endif
}
