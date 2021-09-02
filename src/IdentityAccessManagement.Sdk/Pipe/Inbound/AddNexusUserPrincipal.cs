using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;

namespace IdentityAccessManagement.Sdk.Pipe.Inbound
{
    public class AddNexusUserPrincipal
    {
        private readonly RequestDelegate _next;

        public AddNexusUserPrincipal(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            SaveUserAuthorizationToExecutionContext(context);
            await _next(context);
        }

        private void SaveUserAuthorizationToExecutionContext(HttpContext context)
        {
            var token = context.Request.Headers[Constants.NexusUserAuthorizationHeaderName];
            if (string.IsNullOrWhiteSpace(token)) return;

            // Note! This middleware is used in adapters where we trust that the Business API has validated the user token
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