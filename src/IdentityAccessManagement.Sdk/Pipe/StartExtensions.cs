using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using IdentityAccessManagement.Sdk.Handlers;
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

namespace IdentityAccessManagement.Sdk.Pipe
{
    public static class StartExtensions
    {
        public static void AddNexusIdentityAccessManagement(this IServiceCollection services, string authority, string audience)
        {

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Events.OnTokenValidated = context =>
                    {
                        SaveUserAuthorizationToExecutionContext(context.HttpContext);
                        SaveUserAuthorizationHeaderToExecutionContext(context.HttpContext);

                        FulcrumApplication.Context.ClientPrincipal = context.HttpContext.User; // TODO: Here? Or as middleware?
                        return Task.CompletedTask;
                    };

                    options.Authority = authority; //Base URL to Idp
                    options.Audience = audience;

                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                    options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
                    options.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;

                    //TODO: Add support for introspection (Reference tokens) (https://irmdevdocs.z16.web.core.windows.net/articles/CIAM/satta-upp-nytt-projekt-som-anvander-ciam/skydda-ett-api.html)
                    // if token does not contain a dot, it is a reference token
                    //options.ForwardDefaultSelector = Selector.ForwardReferenceToken("introspection");


                });
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
                SignatureValidator = (t, parameters) => new JwtSecurityToken(t)
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
