using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Authentication.Sdk.Extensions;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using AuthenticationManager = Nexus.Link.Authentication.Sdk.AuthenticationManager;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Error.Logic;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#else
using System.Net.Http;
#endif

namespace Nexus.Link.Authentication.AspNet.Sdk.Handlers
{

    public abstract class TokenValidationHandlerBase : CompatibilityDelegatingHandler
    {
        protected string Issuer;

#if NETCOREAPP
        protected TokenValidationHandlerBase(RequestDelegate next, string issuer) : base(next)
        {
            InternalContract.RequireNotNullOrWhiteSpace(issuer, nameof(issuer));
            Issuer = issuer;
        }
#else
        protected TokenValidationHandlerBase(string issuer)
        {
            InternalContract.RequireNotNullOrWhiteSpace(issuer, nameof(issuer));
            Issuer = issuer;
        }
#endif


        protected override async Task InvokeAsync(CompabilityInvocationContext context)
        {
            var token = GetToken(context);
            if (token != null)
            {
                var tenant = FulcrumApplication.Context.ClientTenant ?? FulcrumApplication.Setup.Tenant;
                FulcrumAssert.IsNotNull(tenant, "Could not verify claims principal, because the application tenant was set to null.");

                // Validate token
                var possiblePlatformServiceTenantFromToken = CheckTokenForPlatformService(token);
                if (possiblePlatformServiceTenantFromToken != null) tenant = possiblePlatformServiceTenantFromToken;

                var publicKey = await GetPublicKeyAsync(tenant);
                if (publicKey == null)
                {
                    Log.LogError($"Could not fetch public key for tenant '{tenant}'");
                    throw new FulcrumUnauthorizedException("See log for more information");
                }

                VerifyTokenAndSetClaimsPrincipal(token, publicKey, tenant, context);
            }
            else
            {
                Log.LogInformation("No token found. This is considered an anonymous call.");
            }

            // Plan B: At least set the calling client name to the calling user agent
            if (FulcrumApplication.Context.CallingClientName == null)
            {
                FulcrumApplication.Context.CallingClientName = GetRequestUserAgent(context);
            }

            await CallNextDelegateAsync(context);
        }

        private Tenant CheckTokenForPlatformService(string token)
        {
            var jwt = AuthenticationManager.ReadTokenNotValidating(token);
            if (jwt?.Claims == null)
            {
                Log.LogWarning("Could not convert token to jwt");
                return null;
            }

            var isPlatformService = jwt.Claims.Any(x => x.Type == "role" && x.Value == NexusAuthenticationRoles.PlatformService);
            if (!isPlatformService) return null;

            // Support legacy issuer for a while
            var orgFromToken = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypeNames.Organization)?.Value
                               ?? jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypeNames.LegacyOrganization)?.Value;
            var envFromToken = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypeNames.Environment)?.Value
                               ?? jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypeNames.LegacyEnvironment)?.Value;
            return new Tenant(orgFromToken, envFromToken);
        }

        protected abstract bool ClaimHasCorrectTenant(ClaimsPrincipal principal, Tenant tenant);

        private void VerifyTokenAndSetClaimsPrincipal(string token, RsaSecurityKey publicKey, Tenant tenant, CompabilityInvocationContext context)
        {
            InternalContract.RequireNotNull(token, nameof(token));
            InternalContract.RequireNotNull(publicKey, nameof(publicKey));

            var claimsPrincipal = AuthenticationManager.ValidateToken(token, publicKey, Issuer);
            if (claimsPrincipal == null)
            {
                Log.LogInformation($"Invalid token: {token}");
                return;
            }

            // Validate tenant
            if (ClaimHasCorrectTenant(claimsPrincipal, tenant))
            {
                SetClaimsPrincipal(claimsPrincipal, context);
            }
        }

        private static void SetClaimsPrincipal(ClaimsPrincipal claimsPrincipal, CompabilityInvocationContext context)
        {
            // Set the ClaimsPrincipal on the current thread.
            Thread.CurrentPrincipal = claimsPrincipal;
            FulcrumApplication.Context.ClientPrincipal = claimsPrincipal;
            FulcrumApplication.Context.CallingClientName = claimsPrincipal.GetClientName();
#if NETCOREAPP
            context.Context.User = claimsPrincipal;
#else
            if (context.RequestMessage.GetRequestContext() != null)
            {
                context.RequestMessage.GetRequestContext().Principal = claimsPrincipal;
            }
#endif
        }

        protected abstract Task<RsaSecurityKey> GetPublicKeyAsync(Tenant tenant);

        private string GetRequestUserAgent(CompabilityInvocationContext context)
        {
            return GetRequestHeaderValues("User-Agent", context)?.FirstOrDefault();
        }

        // Reads the token from the authorization header on the inbound request
        private static string GetToken(CompabilityInvocationContext context)
        {
            string token;

            var headerValue = GetRequestHeaderValues("Authorization", context)?.FirstOrDefault();
            if (headerValue != null)
            {
                // Verify Authorization header contains 'Bearer' scheme
                token = headerValue.ToLowerInvariant().StartsWith("bearer ") ? headerValue.Split(' ')[1] : null;
                if (token != null) return token;
            }

            token = GetRequestQueryValues("api_key", context)?.FirstOrDefault();
            return token;
        }

        private static IEnumerable<string> GetRequestHeaderValues(string headerName,
            CompabilityInvocationContext context)
        {
#if NETCOREAPP
            var request = context?.Context?.Request;
            if (request == null) return null;
            if (!request.Headers.TryGetValue(headerName, out var headerValues)) return null;
            return headerValues;
#else
            var request = context?.RequestMessage;
            if (request == null) return null;

            return !request.Headers.Contains(headerName) ? null : request.Headers.GetValues(headerName);
#endif
        }

        private static IEnumerable<string> GetRequestQueryValues(string name, CompabilityInvocationContext context)
        {
#if NETCOREAPP
            var request = context?.Context?.Request;
            if (request == null) return null;
            if (!request.Query.TryGetValue(name, out var values)) return null;
            return values;
#else
            var request = context?.RequestMessage;
            return request?.GetQueryNameValuePairs().Where(x => x.Key == name).Select(x => x.Value);
#endif
        }

    }
}
