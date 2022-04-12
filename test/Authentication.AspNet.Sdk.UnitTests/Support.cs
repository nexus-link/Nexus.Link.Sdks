#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Authentication.AspNet.Sdk.UnitTests
{
    internal class Support
    {
        public RSACryptoServiceProvider CryptoServiceProvider { get; }

        public Support()
        {
            CryptoServiceProvider = new RSACryptoServiceProvider(AuthenticationManager.RsaKeySizeInBits);
        }

        public AuthenticationToken CreateToken(Tenant tenant, string clientId, IEnumerable<string> roles, bool expired, bool legacyIssuer = false)
        {
            InternalContract.RequireNotNullOrWhiteSpace(clientId, nameof(clientId));
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNull(roles, nameof(roles));

            var privateKey = new RsaSecurityKey(CryptoServiceProvider.ExportParameters(true));


            var tokenHandler = new JwtSecurityTokenHandler();

            var notBefore = DateTime.Today.Subtract(TimeSpan.FromDays(1));
            var expires = expired ? notBefore.AddHours(1) : DateTime.UtcNow.AddHours(1);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, clientId),
                new Claim(ClaimTypeNames.Organization, tenant.Organization),
                new Claim(ClaimTypeNames.Environment, tenant.Environment)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            if (legacyIssuer)
            {
                claims.RemoveAll(x => x.Type == ClaimTypeNames.Organization || x.Type == ClaimTypeNames.Environment);
                claims.Add(new Claim(ClaimTypeNames.LegacyOrganization, tenant.Organization));
                claims.Add(new Claim(ClaimTypeNames.LegacyEnvironment, tenant.Environment));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = AuthenticationManager.AuthServiceIssuer,
                Subject = new ClaimsIdentity(claims),
                NotBefore = notBefore,
                Expires = expires,
                SigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256Signature)
            };
            if (legacyIssuer)
            {
                tokenDescriptor.Issuer = AuthenticationManager.LegacyIssuer;
                tokenDescriptor.Audience = AuthenticationManager.LegacyAudience;
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var result = new AuthenticationToken
            {
                AccessToken = tokenString,
                ExpiresOn = expires
            };
            return result;
        }
    }
}
#endif