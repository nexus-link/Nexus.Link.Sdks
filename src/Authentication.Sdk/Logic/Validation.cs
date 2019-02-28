using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk.Logic
{
    internal static class Validation
    {
        public const string Issuer = "Fulcrum Authentication";
        public const string Audience = "fulcrum";

        public static readonly byte[] PublicSecurityKeyExponent = Convert.FromBase64String("AQAB");
        public static readonly byte[] PublicSecurityKeyModulus = Convert.FromBase64String("oVXcjTON0QARkulUVH/amaak2Tgl/8rkObRUQyPFT50GKxm+bc4QYj5e0ANqbe6AuD4R6EnmjFvEZ1JAZeXUBbJzPt4Lba3Vule0TDybYpE2Ln03hFzgoHr6p0CXkV32cSHupVfqk5sl7kuTLnylcbJjm9ntxPRrD6g4IbUpY4tqNJhU4r/xTlLomkeNEonEIo3nAC/KfKQbcLicrI0hfLObqgIjtHGhuWrlfeWAcziLnEMqJB6C7Y3dP/WufexVNy8EuJYoYjy8b4tMJ/Or/qWUfRFqj1J25KfueUfI7OsHDzrLSqyypD+rYRujxnkcvJgn73JeaXALhUAw7G9oOw==");

        private static readonly RsaSecurityKey PublicKey = new RsaSecurityKey(new RSAParameters
        {
            Exponent = PublicSecurityKeyExponent,
            Modulus = PublicSecurityKeyModulus
        });

        public static ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = Audience,
                    IssuerSigningKey = PublicKey,
                    ValidIssuer = Issuer
                };

                SecurityToken securityToken;
                return tokenHandler.ValidateToken(token, validationParameters, out securityToken);

            }
            catch (SecurityTokenException e)
            {
                throw new FulcrumUnauthorizedException($"Could not validate token: {e.Message}", e);
            }
        }

        public static bool HasRole(AuthenticationRoleEnum role, ClaimsPrincipal principal)
        {
            var roleName = Enum.GetName(typeof(AuthenticationRoleEnum), role);
            var result = principal.IsInRole(roleName);
            return result;
        }

        public static string GetClaimValue(string type, ClaimsPrincipal principal)
        {
            var claim = principal.Identities.First().Claims.FirstOrDefault(c => c.Type == type);
            var claimAudience = principal.Identities.First().Claims.FirstOrDefault(c => c.Type == "aud");
            if (claim != null && claimAudience != null && claimAudience.Value == Audience)
            {
                return claim.Value;
            }
            return null;
        }

    }
}