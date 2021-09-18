using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Pipe;

namespace IdentityAccessManagement.Sdk.Pipe
{
    public static class StartExtensions
    {
        /// <summary>
        /// Adds Nexus Identity Access Management capability to inbound pipe to handle authentication.
        /// </summary>
        /// <remarks>Also use <see cref="UseNexusIdentityAccessManagement"/> in your <code>Configure(IApplicationBuilder)</code></remarks>
        public static void AddNexusIdentityAccessManagement(this IServiceCollection services, string authority, Action<JwtBearerOptions> jwtBearerOptionsAction = null)
        {
            AddNexusIdentityAccessManagement(services, authority, (List<string>)null, jwtBearerOptionsAction);
        }

        /// <summary>
        /// Adds Nexus Identity Access Management capability to inbound pipe to handle authentication.
        /// </summary>
        /// <remarks>Also use <see cref="UseNexusIdentityAccessManagement"/> in your <code>Configure(IApplicationBuilder)</code></remarks>
        public static void AddNexusIdentityAccessManagement(this IServiceCollection services, string authority, string audience, Action<JwtBearerOptions> jwtBearerOptionsAction = null)
        {
            AddNexusIdentityAccessManagement(services, authority, new List<string> { audience }, jwtBearerOptionsAction);
        }

        /// <summary>
        /// Adds Nexus Identity Access Management capability to inbound pipe to handle authentication.
        /// </summary>
        /// <remarks>Also use <see cref="UseNexusIdentityAccessManagement"/> in your <code>Configure(IApplicationBuilder)</code></remarks>
        public static void AddNexusIdentityAccessManagement(this IServiceCollection services, string authority, List<string> validAudiences, Action<JwtBearerOptions> jwtBearerOptionsAction = null)
        {

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority; // Base URL to Idp
                    options.Audience = validAudiences?.Count == 1 ? validAudiences.First() : null;
                    options.TokenValidationParameters.ValidAudiences = validAudiences;
                    options.TokenValidationParameters.ValidateAudience = validAudiences != null && validAudiences.Any();

                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                    options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name; // TODO: From IRM docs
                    options.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;

                    //TODO: Add support for introspection (Reference tokens) (https://irmdevdocs.z16.web.core.windows.net/articles/CIAM/satta-upp-nytt-projekt-som-anvander-ciam/skydda-ett-api.html)
                    // if token does not contain a dot, it is a reference token
                    //options.ForwardDefaultSelector = Selector.ForwardReferenceToken("introspection");

                    jwtBearerOptionsAction?.Invoke(options);
                });
        }

        /// <summary>
        /// Activates Nexus Identity Access Management capability to inbound pipe to handle authentication.
        /// Adds <code>UseAuthentication()</code> and <code>UseAuthorization</code> to <see cref="app"/>.
        /// </summary>
        /// <remarks>Also use <see cref="AddNexusIdentityAccessManagement"/> in your <code>ConfigureServices(IServiceCollection)</code></remarks>
        public static void UseNexusIdentityAccessManagement(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMiddleware<SaveNexusAuthorizationToExecutionContext>();
        }
    }

    public class SaveNexusAuthorizationToExecutionContext
    {
        private readonly RequestDelegate _next;

        public SaveNexusAuthorizationToExecutionContext(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            FulcrumApplication.Context.ClientPrincipal = context.User;
            SaveUserAuthorizationToExecutionContext(context);
            SaveUserAuthorizationHeaderToExecutionContext(context);

            await _next.Invoke(context);
        }

        private static void SaveUserAuthorizationToExecutionContext(HttpContext context)
        {
            var userAuthorization = ExtractUserAuthorizationFromHeader(context);
            if (userAuthorization == null) return;
            FulcrumApplication.Context.ValueProvider.SetValue(Constants.NexusUserAuthorizationKeyName, userAuthorization);
        }

        private static string ExtractUserAuthorizationFromHeader(HttpContext context)
        {
            var request = context.Request;

            FulcrumAssert.IsNotNull(request, CodeLocation.AsString());
            if (request == null) return null;
            var authorizationHeaderExists = request.Headers.TryGetValue("Authorization", out var authorizationValues);

            if (!authorizationHeaderExists) return null;

            var authorizationArray = authorizationValues.ToArray();
            if (authorizationArray.Length > 1)
            {
                // ReSharper disable once UnusedVariable
                var message = $"There was more than one authorization value in the header: {string.Join(", ", authorizationArray)}. The first one was picked as User Authorization";
                Log.LogWarning(message);
            }

            return authorizationArray[0];
        }

        private static void SaveUserAuthorizationHeaderToExecutionContext(HttpContext context)
        {
            var token = context.Request.Headers[Constants.NexusUserAuthorizationHeaderName];
            if (string.IsNullOrWhiteSpace(token)) return;

            // Note! This middleware is used in adapters where we trust that the Business API has validated the user token
            //TODO: Should we do this? How to handle it?
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateActor = false,
                ValidateLifetime = false,
                ValidateTokenReplay = false,
                SignatureValidator = (t, parameters) => new JwtSecurityToken(t),
                NameClaimType = JwtClaimTypes.Name
            };

            var validator = new JwtSecurityTokenHandler();
            if (validator.CanReadToken(token))
            {
                try
                {
                    var principal = validator.ValidateToken(token, validationParameters, out _);
                    FulcrumApplication.Context.UserPrincipal = principal;
                }
                catch (Exception e)
                {
                    Log.LogError($"Unable to read user token on header {Constants.NexusUserAuthorizationHeaderName}", e);
                }
            }
        }
    }
}
