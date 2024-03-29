﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Authentication.Sdk.Logic
{
    internal static class Validation
    {
        public const string NexusIssuer = "nexus";
        public const string AuthServiceIssuer = "Nexus Authentication Service";
        public const string LegacyIssuer = "Fulcrum Authentication";
        public const string LegacyAudience = "fulcrum";

        public static ClaimsPrincipal ValidateToken(string token, RsaSecurityKey publicKey, string issuer)
        {
            return ValidateToken(token, publicKey, issuer, false);
        }

        public static ClaimsPrincipal ValidateToken(string token, RsaSecurityKey publicKey, string issuer, bool ignoreExpiration)
        {
            try
            {
                var validateLifetime = !ignoreExpiration;
                var securityTokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = publicKey,
                    ValidIssuer = issuer,
                    ValidateLifetime = validateLifetime
                };
                var claimsPrincipal = securityTokenHandler.ValidateToken(token, validationParameters, out var securityToken);
                return claimsPrincipal;
            }
            catch (Exception e) when (e is SecurityTokenException || e is ArgumentException)
            {
                throw new FulcrumUnauthorizedException("Could not validate token: " + e.Message, e);
            }
        }

        public static bool HasRole(string role, ClaimsPrincipal principal)
        {
            var result = principal.IsInRole(role);
            return result;
        }

        public static string GetClaimValue(string type, ClaimsPrincipal principal)
        {
            var claim = principal.Identities.First().Claims.FirstOrDefault(c => c.Type == type);
            return claim?.Value;
        }
    }
}