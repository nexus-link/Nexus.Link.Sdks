using System;
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
using AuthenticationManager = Nexus.Link.Authentication.Sdk.AuthenticationManager;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;

#if NETCOREAPP
using Nexus.Link.Libraries.Web.AspNet.Logging;
using Nexus.Link.Contracts.Misc.AspNet.Sdk;
using Microsoft.AspNetCore.Http;
#else
using System.Net.Http;
#endif

namespace Nexus.Link.Authentication.AspNet.Sdk.Handlers
{

    public abstract class TokenValidationHandlerBase : CompatibilityDelegatingHandlerWithCancellationSupport
    {
        protected string Issuer;

#if NETCOREAPP
        private readonly IReentryAuthenticationService _reentryAuthenticationService;
        protected TokenValidationHandlerBase(RequestDelegate next, string issuer, IReentryAuthenticationService reentryAuthenticationService) : base(next)
        {
            InternalContract.RequireNotNullOrWhiteSpace(issuer, nameof(issuer));
            Issuer = issuer;
            _reentryAuthenticationService = reentryAuthenticationService;
        }
#else
        protected TokenValidationHandlerBase(string issuer)
        {
            InternalContract.RequireNotNullOrWhiteSpace(issuer, nameof(issuer));
            Issuer = issuer;
        }
#endif


        protected override async Task InvokeAsync(CompabilityInvocationContext context, CancellationToken cancellationToken)
        {
            var token = RequestHelper.GetAuthorizationBearerTokenOrApiKey(context);
            if (token != null)
            {
                var tenant = FulcrumApplication.Context.ClientTenant ?? FulcrumApplication.Setup.Tenant;
                FulcrumAssert.IsNotNull(tenant, "Could not verify claims principal, because the application tenant was set to null.");

                // Validate token
                var possiblePlatformServiceTenantFromToken = CheckTokenForPlatformService(token);
                if (possiblePlatformServiceTenantFromToken != null) tenant = possiblePlatformServiceTenantFromToken;

                var publicKey = await GetPublicKeyAsync(tenant, cancellationToken);
                if (publicKey == null)
                {
                    Log.LogError($"Could not fetch public key for tenant '{tenant}'");
                    throw new FulcrumUnauthorizedException("See log for more information");
                }

#if NETCOREAPP
                var ignoreExpiration = await ShouldWeIgnoreExpirationAsync(tenant, context.Context, cancellationToken);
#else
                var ignoreExpiration = false;
#endif
                VerifyTokenAndSetClaimsPrincipal(token, publicKey, tenant, context, ignoreExpiration);
            }
            else
            {
                Log.LogInformation("No token found. This is considered an anonymous call.");
            }

            // Plan B: At least set the calling client name to the calling user agent
            if (FulcrumApplication.Context.CallingClientName == null)
            {
                FulcrumApplication.Context.CallingClientName = RequestHelper.GetRequestUserAgent(context);
            }

            await CallNextDelegateAsync(context, cancellationToken);
        }

        private static Tenant CheckTokenForPlatformService(string token)
        {
            var jwt = AuthenticationManager.ReadTokenNotValidating(token);
            if (jwt?.Claims == null) return null;

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

#if NETCOREAPP

        private bool _hasWarnedAboutMissingReentryTokenService;
        private async Task<bool> ShouldWeIgnoreExpirationAsync(Tenant tenant,
            HttpContext context, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNull(context, nameof(context));

            if (string.IsNullOrWhiteSpace(FulcrumApplication.Context.ReentryAuthentication)) return false;
            if (_reentryAuthenticationService != null)
            {
                return await _reentryAuthenticationService.ValidateAsync(FulcrumApplication.Context.ReentryAuthentication, context, cancellationToken);
            }

            var request = context.Request;
            var message = $"The request {request.ToLogString()} for tenant {tenant.ToLogString()} had a" +
                          $" {nameof(FulcrumApplication.Context.ReentryAuthentication)}, but no {nameof(IReentryAuthenticationService)}" +
                          $" was configured for the {nameof(TokenValidationHandler)}.";
            var severityLevel = LogSeverityLevel.Verbose;
            if (!_hasWarnedAboutMissingReentryTokenService)
            {
                severityLevel = LogSeverityLevel.Warning;
                _hasWarnedAboutMissingReentryTokenService = true;
            }

            Log.LogOnLevel(severityLevel, message);
            return await Task.FromResult(false);
        }
#endif

        private void VerifyTokenAndSetClaimsPrincipal(string token, RsaSecurityKey publicKey, Tenant tenant, CompabilityInvocationContext context, bool ignoreExpiration = false)
        {
            InternalContract.RequireNotNullOrWhiteSpace(token, nameof(token));
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNull(context, nameof(context));
            InternalContract.RequireNotNull(publicKey, nameof(publicKey));

            var claimsPrincipal = TryGetClaimsPrincipal(token, publicKey, ignoreExpiration);
            if (claimsPrincipal == null)
            {
                Log.LogInformation($"Invalid token: {token}. Issuer: {Issuer}.");
                return;
            }

            // Validate tenant
            if (ClaimHasCorrectTenant(claimsPrincipal, tenant))
            {
                SetClaimsPrincipal(claimsPrincipal, context);
            }
        }

        private ClaimsPrincipal TryGetClaimsPrincipal(string token, RsaSecurityKey publicKey, bool ignoreExpiration = false)
        {
            try
            {
                var claimsPrincipal = AuthenticationManager.ValidateToken(token, publicKey, Issuer, ignoreExpiration);
                return claimsPrincipal;
            }
            catch (Exception e1)
            {
                Log.LogVerbose($"Failed to validate token with issuer {Issuer}. Error message: {e1.Message}");

                // For a while, support legacy tokens as well
                try
                {
                    var claimsPrincipal = AuthenticationManager.ValidateToken(token, publicKey, AuthenticationManager.LegacyIssuer);
                    return claimsPrincipal;
                }
                catch (Exception e2)
                {
                    Log.LogVerbose(
                        $"Failed to validate token with legacy issuer {AuthenticationManager.LegacyIssuer}. Error message: {e2.Message}");
                    return null;
                }
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

        protected abstract Task<RsaSecurityKey> GetPublicKeyAsync(Tenant tenant, CancellationToken cancellationToken = default);
    }
}
