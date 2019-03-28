using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Authentication.Sdk.Extensions;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#else
using System.Net.Http;
#endif

namespace Nexus.Link.Authentication.AspNet.Sdk.Handlers
{

    public abstract class TokenValidationHandlerBase : CompatibilityDelegatingHandler
    {
        protected string FundamentalsServiceBaseUrl;
        protected string Issuer;

#if NETCOREAPP
        /// <inheritdoc />
        protected TokenValidationHandlerBase(RequestDelegate next, string fundamentalsServiceBaseUrl, string issuer) : base(next)
        {
            FundamentalsServiceBaseUrl = fundamentalsServiceBaseUrl;
            Issuer = issuer;
        }
#else
        protected TokenValidationHandlerBase(string fundamentalsServiceBaseUrl, string issuer)
        {
            FundamentalsServiceBaseUrl = fundamentalsServiceBaseUrl;
            Issuer = issuer;
        }
#endif


        protected override async Task InvokeAsync(CompabilityInvocationContext context)
        {
            var token = GetToken(context);
            if (token != null)
            {
                await VerifyTokenAndSetClaimsPrincipal(token, context);
            }

            // Plan B: At least set the calling client name to the calling user agent
            if (FulcrumApplication.Context.CallingClientName == null)
            {
                FulcrumApplication.Context.CallingClientName = GetRequestUserAgent(context);
            }

            await CallNextDelegateAsync(context);
        }

        protected abstract Task<string> FetchPublicKeyXmlAsync(Tenant tenant);
        protected abstract bool ClaimHasCorrectTenant(ClaimsPrincipal principal, Tenant tenant);

        private async Task VerifyTokenAndSetClaimsPrincipal(string token, CompabilityInvocationContext context)
        {
            var tenant = FulcrumApplication.Context.ClientTenant ?? FulcrumApplication.Setup.Tenant;
            if (tenant == null)
            {
                Log.LogCritical("Could not verify claims principal, because the application tenant was set to null.");
                return;
            }

            // Validate token
            var publicKey = await FetchPublicKeyXmlAsync(tenant);
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
                token = headerValue.StartsWith("Bearer ") ? headerValue.Split(' ')[1] : null;
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