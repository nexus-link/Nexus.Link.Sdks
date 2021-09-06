using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using IdentityAccessManagement.Sdk.Pipe;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Application;

namespace IdentityAccessManagement.Sdk.UnitTest
{
    public class TestStartup
    {
        public const string ClientName = "fuu";
        public const string UserName = "baa";
        public const string Issuer = "unittest";
        public const string Audience = "home";

        public static AuthenticationHeaderValue AuthorizationHeader { get; private set; }
        public static string UserAuthorizationHeader { get; private set; }


        private readonly IConfiguration _configuration;
        const int RsaKeySizeInBits = 2048;

        public TestStartup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            FulcrumApplicationHelper.UnitTestSetup("unitests");

            var rsaProvider = new RSACryptoServiceProvider(RsaKeySizeInBits);
            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsaProvider.ExportParameters(true)), "RS256");
            var validationKey = new RsaSecurityKey(rsaProvider.ExportParameters(false));

            var clientJwt = CreateJwt(ClientName, signingCredentials);
            AuthorizationHeader = new AuthenticationHeaderValue("Bearer", clientJwt);
            var userJwt = CreateJwt(UserName, signingCredentials);
            UserAuthorizationHeader = userJwt;

            services.AddNexusIdentityAccessManagement(Issuer, Audience);
            services.AddMvc(options => options.EnableEndpointRouting = false);
        }

        private static string CreateJwt(string clientName, SigningCredentials signingCredentials)
        {
            var jwt = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
            {
                SigningCredentials = signingCredentials,
                Expires = DateTime.UtcNow.AddHours(1),
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "consumer"),
                    new Claim(ClaimTypes.Name, clientName)
                }),
                Issuer = Issuer,
                Audience = Audience,
            });
            return jwt;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseNexusIdentityAccessManagement();
            app.UseMvc();
        }
    }
}