using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Xml;
using IdentityAccessManagement.Sdk.Pipe;
using Microsoft.IdentityModel.Tokens;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace IdentityAccessManagement.Sdk.Handlers
{
    public class IamAuthenticationManager
    {
        /// <summary>
        /// The token validation parameters used to setup AddJwtBearer in <see cref="StartExtensions.AddNexusIdentityAccessManagement(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.IdentityModel.Tokens.SecurityKey,string,string)"/>
        /// </summary>
        public static TokenValidationParameters TokenValidationParameters { get; internal set; }

        public static RsaSecurityKey CreateRsaSecurityKeyFromXmlString(string publicKeyXml, int rsaKeySizeInBits)
        {
            var provider = new RSACryptoServiceProvider(rsaKeySizeInBits);
            try
            {
                provider.FromXmlString(publicKeyXml);
            }
            catch (PlatformNotSupportedException)
            {
                // Support in .NET Core is not planned until .NET Core 3.0, so we use a workaround
                // https://github.com/dotnet/core/issues/874
                FromXmlString(provider, publicKeyXml);
            }
            return new RsaSecurityKey(provider.ExportParameters(false));
        }

        /// <summary>
        /// Workaround found at https://github.com/dotnet/core/issues/874
        /// </summary>
        private static void FromXmlString(RSA rsa, string xmlString)
        {
            var parameters = new RSAParameters();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            var name = xmlDoc.DocumentElement?.Name;
            if (name == "RSAKeyValue")
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus": parameters.Modulus = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText); break;
                        case "Exponent": parameters.Exponent = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText); break;
                        case "P": parameters.P = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText); break;
                        case "Q": parameters.Q = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText); break;
                        case "DP": parameters.DP = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText); break;
                        case "DQ": parameters.DQ = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText); break;
                        case "InverseQ": parameters.InverseQ = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText); break;
                        case "D": parameters.D = string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText); break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            rsa.ImportParameters(parameters);
        }

        public static ClaimsPrincipal ValidateToken(string token)
        {
            FulcrumAssert.IsNotNull(TokenValidationParameters, CodeLocation.AsString(), $"Use {nameof(StartExtensions)}.{nameof(StartExtensions.AddNexusIdentityAccessManagement)}.");
            try
            {
                var securityTokenHandler = new JwtSecurityTokenHandler();
                return securityTokenHandler.ValidateToken(token, TokenValidationParameters, out _);
            }
            catch (Exception e) when (e is SecurityTokenException || e is ArgumentException)
            {
                throw new FulcrumUnauthorizedException("Could not validate token: " + e.Message, e);
            }
        }
    }
}