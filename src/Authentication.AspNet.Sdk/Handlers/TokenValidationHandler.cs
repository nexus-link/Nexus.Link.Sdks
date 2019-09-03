using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Authentication.Sdk.Extensions;
using Nexus.Link.Libraries.Core.Assert;
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
        private readonly RsaSecurityKey _publicKey;

#if NETCOREAPP
        public TokenValidationHandler(RequestDelegate next, RsaSecurityKey publicKey)
            : base(next, AuthenticationManager.AuthServiceIssuer)
        {
            InternalContract.RequireNotNull(publicKey, nameof(publicKey));
            _publicKey = publicKey;
            FulcrumAssert.IsNotNull(_publicKey);
        }
        public TokenValidationHandler(RequestDelegate next, string publicKeyAsXmlString)
            : base(next, AuthenticationManager.AuthServiceIssuer)
        {
            InternalContract.RequireNotNull(publicKeyAsXmlString, nameof(publicKeyAsXmlString));
            _publicKey = AuthenticationManager.CreateRsaSecurityKeyFromXmlString(publicKeyAsXmlString);
            FulcrumAssert.IsNotNull(_publicKey);
        }
#else
        public TokenValidationHandler(RsaSecurityKey publicKey)
            : base(AuthenticationManager.AuthServiceIssuer)
        {
            InternalContract.RequireNotNull(publicKey, nameof(publicKey));
            _publicKey = publicKey;
        }
        public TokenValidationHandler(string publicKeyAsXmlString)
            : base(AuthenticationManager.AuthServiceIssuer)
        {
            InternalContract.RequireNotNull(publicKeyAsXmlString, nameof(publicKeyAsXmlString));
            _publicKey = AuthenticationManager.CreateRsaSecurityKeyFromXmlString(publicKeyAsXmlString);
        }
#endif

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

        protected override Task<RsaSecurityKey> GetPublicKeyAsync(Tenant tenant)
        {
            return Task.FromResult(_publicKey);
        }
    }

#if NETCOREAPP
    public static class TokenValidationHandlerExtension
    {
        public static IApplicationBuilder UseNexusTokenValidationHandler(
            this IApplicationBuilder builder,
            RsaSecurityKey publicKey)
        {
            return builder.UseMiddleware<TokenValidationHandler>(publicKey);
        }
        public static IApplicationBuilder UseNexusTokenValidationHandler(
            this IApplicationBuilder builder,
            string publicKeyAsXmlString)
        {
            return builder.UseMiddleware<TokenValidationHandler>(publicKeyAsXmlString);
        }
    }
#endif
}
