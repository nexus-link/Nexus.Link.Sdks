using IdentityAccessManagement.Sdk.Handlers;
using IdentityAccessManagement.Sdk.Pipe.Inbound;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Assert;

namespace IdentityAccessManagement.Sdk.Pipe
{
    public static class StartExtensions
    {
        public static void AddNexusIdentityAccessManagement(this IServiceCollection services, SecurityKey validationKey, string issuer, string audience)
        {
            InternalContract.RequireNotNull(validationKey, nameof(validationKey));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = validationKey,
                        ValidIssuer = issuer,
                        ValidateIssuer = issuer != null,
                        ValidAudience = audience,
                        ValidateAudience = audience != null
                    };
                });
        }

        public static void AddNexusIdentityAccessManagement(this IServiceCollection services, string publicKeyXml, int rsaKeySizeInBits, string issuer, string audience)
        {
            InternalContract.RequireNotNullOrWhiteSpace(publicKeyXml, nameof(publicKeyXml));

            var publicKey = IamAuthenticationManager.CreateRsaSecurityKeyFromXmlString(publicKeyXml, rsaKeySizeInBits);
            services.AddNexusIdentityAccessManagement(publicKey, issuer, audience);
        }

        public static void UseNexusIdentityAccessManagement(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMiddleware<AddNexusClientPrincipal>();
            app.UseMiddleware<AddNexusUserPrincipal>();
        }
    }
}
