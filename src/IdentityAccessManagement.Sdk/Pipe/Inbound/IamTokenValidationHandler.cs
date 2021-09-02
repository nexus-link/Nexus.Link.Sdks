using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityAccessManagement.Sdk.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace IdentityAccessManagement.Sdk.Pipe.Inbound
{
    public class IamTokenValidationHandler
    {
        private readonly RsaSecurityKey _publicKey;
        protected string Issuer;
        private readonly RequestDelegate _next;
        private readonly IContextValueProvider _provider;
        private static string ContextKey = "UserId";


        public IamTokenValidationHandler(RequestDelegate next, RsaSecurityKey publicKey, string issuer)
        {
            InternalContract.RequireNotNull(publicKey, nameof(publicKey));
            _next = next;
            _publicKey = publicKey;
            Issuer = issuer;
            _provider = new AsyncLocalContextValueProvider();
            FulcrumAssert.IsNotNull(_publicKey);
        }

        public IamTokenValidationHandler(RequestDelegate next, string issuer, string publicKeyAsXmlString, int rsaKeySizeInBits = 2048)
        {
            InternalContract.RequireNotNull(publicKeyAsXmlString, nameof(publicKeyAsXmlString));
            InternalContract.RequireGreaterThan(0, rsaKeySizeInBits, nameof(rsaKeySizeInBits));

            _next = next;
            Issuer = issuer;
            _publicKey = IamAuthenticationManager.CreateRsaSecurityKeyFromXmlString(publicKeyAsXmlString, rsaKeySizeInBits);
            _provider = new AsyncLocalContextValueProvider();
            FulcrumAssert.IsNotNull(_publicKey);
        }

        protected async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var token = GetToken(context, "Authorization");
            if (token != null)
            {
                ValidateTokenAndSetClaimsPrincipal(context, token);

                if (FulcrumApplication.Context.ClientPrincipal == null)
                {
                    //Token was end user. Set user id 
                    FulcrumAssert.IsNotNull(FulcrumApplication.Context.UserPrincipal);
                    MaybeSetUserId();
                }
                else
                {
                    FulcrumAssert.IsNotNull(FulcrumApplication.Context.ClientPrincipal);
                    //Token was system: Set UserPrincipal
                    var userToken = GetToken(context, Constants.NexusTranslatedUserIdHeaderName);
                    if (userToken != null)
                    {
                        ValidateTokenAndSetClaimsPrincipal(context, token);
                    }
                }
            }
            else
            {
                Log.LogInformation("No token found. This is considered an anonymous call.");
            }

            //TODO: Maybe set callingClientName
            //// Plan B: At least set the calling client name to the calling user agent
            //FulcrumApplication.Context.CallingClientName ??= GetRequestUserAgent(context);

            await _next(context);
        }

        private void ValidateTokenAndSetClaimsPrincipal(HttpContext context, string token)
        {
            var tenant = FulcrumApplication.Context.ClientTenant ?? FulcrumApplication.Setup.Tenant;
            FulcrumAssert.IsNotNull(tenant, "Could not verify claims principal, because the application tenant was set to null.");

            if (_publicKey == null)
            {
                Log.LogError($"{nameof(_publicKey)} was unexpectedly null");
                throw new FulcrumUnauthorizedException("See log for more information");
            }

            VerifyTokenAndSetClaimsPrincipal(token, _publicKey, tenant, context);
        }

        private void MaybeSetUserId()
        {
            var principal = FulcrumApplication.Context.UserPrincipal as ClaimsPrincipal;
            if (principal == null) return;

            //TODO: Handle this
            var userId = principal.Claims.FirstOrDefault(claim => claim.Type == "UserId");
            if (userId == null) return;

            _provider.SetValue(ContextKey, userId.Value);
        }

        private static string GetToken(HttpContext context, string headerName)
        {
            var headerValue = GetRequestHeaderValues(headerName, context)?.FirstOrDefault();
            if (headerValue == null) return null;

            // Verify Authorization header contains 'Bearer' scheme
            var token = headerValue.ToLowerInvariant().StartsWith("bearer ") ? headerValue.Split(' ')[1] : null;
            return token;
        }

        private static IEnumerable<string> GetRequestHeaderValues(string headerName,
        HttpContext context)
        {
            var request = context.Request;
            if (request == null) return null;
            if (!request.Headers.TryGetValue(headerName, out var headerValues)) return null;
            return headerValues;
        }

        private void VerifyTokenAndSetClaimsPrincipal(string token, RsaSecurityKey publicKey, Tenant tenant, HttpContext context)
        {
            InternalContract.RequireNotNullOrWhiteSpace(token, nameof(token));
            InternalContract.RequireNotNull(publicKey, nameof(publicKey));

            ClaimsPrincipal claimsPrincipal = null;
            try
            {
                claimsPrincipal = IamAuthenticationManager.ValidateToken(token, publicKey, Issuer);
            }
            catch (Exception e1)
            {
                Log.LogVerbose($"Failed to validate token with issuer {Issuer}. Error message: {e1.Message}");
            }
            if (claimsPrincipal == null)
            {
                Log.LogInformation($"Invalid token: {token}. Issuer: {Issuer}.");
                return;
            }

            SetClaimsPrincipal(claimsPrincipal, context);
        }

        private static void SetClaimsPrincipal(ClaimsPrincipal claimsPrincipal, HttpContext context)
        {
            // Set the ClaimsPrincipal on the current thread.
            Thread.CurrentPrincipal = claimsPrincipal;
            //TODO: Handle if principal to set is ClientPrincipal or user principal. Maybe to different handlers?
            FulcrumApplication.Context.ClientPrincipal = claimsPrincipal;
            FulcrumApplication.Context.CallingClientName = claimsPrincipal.GetClientName();

            context.User = claimsPrincipal;
        }

        // TODO: Should we use this?
        protected bool ClaimHasCorrectTenant(ClaimsPrincipal claimsPrincipal, Tenant tenant)
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


    public static class IamTokenValidationHandlerExtension
    {
        public static IApplicationBuilder UseIamTokenValidationHandler(
            this IApplicationBuilder builder,
            RsaSecurityKey publicKey,
            string issuer)
        {
            return builder.UseMiddleware<IamTokenValidationHandler>(publicKey, issuer);
        }
        public static IApplicationBuilder UseIamTokenValidationHandler(
            this IApplicationBuilder builder,
            string publicKeyAsXmlString,
            string issuer)
        {
            return builder.UseMiddleware<IamTokenValidationHandler>(publicKeyAsXmlString, issuer);
        }
    }
}