using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Nexus.Link.Authentication.Sdk.Logic;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk
{
    /// <summary>
    /// Authentication helper for tokens used in the "Authentication as service"
    /// </summary>
    public class AuthenticationManager : IJwtTokenHandler
    {
        private static readonly string Namespace = typeof(AuthenticationManager).Namespace;
        private static readonly HttpClient HttpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());
        private static readonly ConcurrentDictionary<string, TokenCache> TokenCaches = new ConcurrentDictionary<string, TokenCache>();
        private readonly TokenCache _tokenCache;
        public Tenant Tenant { get; }
        public Uri ServiceUri { get; }
        private readonly string _type;
        private readonly bool _legacyIssuer;

        public static string LegacyIssuer => Validation.LegacyIssuer;
        public static string LegacyAudience => Validation.LegacyAudience;
        public static string NexusIssuer => Validation.NexusIssuer;
        public static string AuthServiceIssuer => Validation.AuthServiceIssuer;

        public const int RsaKeySizeInBits = 2048;


        [Obsolete("No use for serviceCredentials anymore", true)]
        // ReSharper disable once UnusedParameter.Local
        public AuthenticationManager(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials) : this(tenant, serviceUri)
        {
        }

        public AuthenticationManager(Tenant tenant, string serviceBaseUrl) : this(tenant, serviceBaseUrl, false)
        {
        }

        public AuthenticationManager(Tenant tenant, string serviceBaseUrl, bool legacyIssuer)
            : this("AuthService", tenant, serviceBaseUrl, $"api/v1/{tenant.Organization}/{tenant.Environment}/Authentication/")
        {
            _legacyIssuer = legacyIssuer;
        }

        protected AuthenticationManager(string type, Tenant tenant, string serviceBaseUrl, string path)
        : this(type, tenant, serviceBaseUrl, path, false)
        { }

        protected AuthenticationManager(string type, Tenant tenant, string serviceBaseUrl, string path, bool resetCache)
        {
            InternalContract.RequireNotNullOrWhiteSpace(type, nameof(type));
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceBaseUrl, nameof(serviceBaseUrl));
            InternalContract.RequireNotNullOrWhiteSpace(path, nameof(path));

            _type = type;
            Tenant = tenant;
            var cacheKey = path;
            if (!TokenCaches.TryGetValue(cacheKey, out _tokenCache) || resetCache)
            {
                _tokenCache = new TokenCache();
                TokenCaches[cacheKey] = _tokenCache;
            }

            if (!serviceBaseUrl.EndsWith("/")) serviceBaseUrl += "/";
            if (!path.EndsWith("/")) path += "/";
            var baseUri = new Uri(serviceBaseUrl);
            ServiceUri = new Uri(baseUri, path);
        }

        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNull(tokenCredentials, nameof(tokenCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(minimumExpirationSpan, nameof(minimumExpirationSpan));
            InternalContract.RequireNotNull(maximumExpirationSpan, nameof(maximumExpirationSpan));

            var authentication = new AuthenticationManager(tenant, serviceUri);
            return await authentication.GetJwtTokenAsync(tokenCredentials, minimumExpirationSpan, maximumExpirationSpan, cancellationToken);
        }

        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, CancellationToken cancellationToken = default)
        {
            return await GetJwtTokenAsync(tenant, serviceUri, tokenCredentials, minimumExpirationSpan, TimeSpan.FromHours(24), cancellationToken);
        }

        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, CancellationToken cancellationToken = default)
        {
            return await GetJwtTokenAsync(tenant, serviceUri, tokenCredentials, TimeSpan.FromHours(1), TimeSpan.FromHours(24), cancellationToken);
        }

        public async Task<AuthenticationToken> GetJwtTokenAsync(IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(tokenCredentials, nameof(tokenCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientId, nameof(tokenCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientSecret, nameof(tokenCredentials.ClientSecret));
            InternalContract.RequireNotNull(minimumExpirationSpan, nameof(minimumExpirationSpan));
            InternalContract.RequireNotNull(maximumExpirationSpan, nameof(maximumExpirationSpan));

            var token = _tokenCache.Get(_type, tokenCredentials, minimumExpirationSpan);
            if (token != null) return token;
            token = await RequestAndCacheJwtTokenAsync(tokenCredentials, maximumExpirationSpan, cancellationToken);
            return token;
        }

        public async Task<AuthenticationToken> GetJwtTokenAsync(IAuthenticationCredentials tokenCredentials, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(tokenCredentials, nameof(tokenCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientId, nameof(tokenCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientSecret, nameof(tokenCredentials.ClientSecret));

            return await GetJwtTokenAsync(tokenCredentials, TimeSpan.FromHours(1), TimeSpan.FromHours(24), cancellationToken);
        }

        public static ITokenRefresherWithServiceClient CreateTokenRefresher(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));

            var authentication = new AuthenticationManager(tenant, serviceUri);
            return authentication.CreateTokenRefresher(tokenCredentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        public static ITokenRefresherWithServiceClient CreateTokenRefresher(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));

            var authentication = new AuthenticationManager(tenant, serviceUri);
            return authentication.CreateTokenRefresher(tokenCredentials);
        }

        public ITokenRefresherWithServiceClient CreateTokenRefresher(IAuthenticationCredentials credentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientId, nameof(credentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientSecret, nameof(credentials.ClientSecret));
            InternalContract.RequireNotNull(minimumExpirationSpan, nameof(minimumExpirationSpan));
            InternalContract.RequireNotNull(maximumExpirationSpan, nameof(maximumExpirationSpan));

            return new TokenRefresher(this, credentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        public ITokenRefresherWithServiceClient CreateTokenRefresher(IAuthenticationCredentials credentials)
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientId, nameof(credentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientSecret, nameof(credentials.ClientSecret));

            return new TokenRefresher(this, credentials);
        }

        /// <summary>
        /// Validates a token.
        /// </summary>
        /// <param name="token">The JWT string</param>
        /// <param name="publicKey">The public part of the RSA key used to sign the JWT</param>
        /// <param name="issuer">Either Nexus services (<see cref="NexusIssuer"/>) or Auth as a service (<see cref="AuthServiceIssuer"/>)</param>
        public static ClaimsPrincipal ValidateToken(string token, RsaSecurityKey publicKey, string issuer)
        {
            InternalContract.RequireNotNullOrWhiteSpace(token, nameof(token));
            InternalContract.RequireNotNull(publicKey, nameof(publicKey));

            return ValidateToken(token, publicKey, issuer, false);
        }

        public static ClaimsPrincipal ValidateToken(string token, RsaSecurityKey publicKey, string issuer, bool ignoreExpiration)
        {
            InternalContract.RequireNotNullOrWhiteSpace(token, nameof(token));
            InternalContract.RequireNotNull(publicKey, nameof(publicKey));

            return Validation.ValidateToken(token, publicKey, issuer, ignoreExpiration);
        }

        /// <summary>
        /// Validates a token.
        /// </summary>
        /// <param name="token">The JWT string</param>
        /// <param name="publicKeyXml">The public part of the RSA key used to sign the JWT</param>
        /// <param name="issuer">Either Nexus services (<see cref="NexusIssuer"/>) or Auth as a service (<see cref="AuthServiceIssuer"/>)</param>
        public static ClaimsPrincipal ValidateToken(string token, string publicKeyXml, string issuer)
        {
            InternalContract.RequireNotNullOrWhiteSpace(token, nameof(token));
            InternalContract.RequireNotNullOrWhiteSpace(publicKeyXml, nameof(publicKeyXml));

            var publicKey = CreateRsaSecurityKeyFromXmlString(publicKeyXml);
            return ValidateToken(token, publicKey, issuer);
        }

        public async Task<string> GetPublicKeyXmlAsync(CancellationToken token = default)
        {
            return await GetPublicKeyXmlAsync(Tenant, ServiceUri.AbsolutePath, token);
        }

        public async Task<RsaSecurityKey> GetPublicRsaKeyAsync(CancellationToken token = default)
        {
            return await GetPublicRsaKeyAsync(Tenant, ServiceUri.AbsolutePath, token);
        }


        public static JwtSecurityToken ReadTokenNotValidating(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                return jsonToken;
            }
            catch (Exception)
            {
                Log.LogWarning($"Could not read token '{token}' as JWT");
                return null;
            }
        }

        private static readonly MemoryCache PublicKeyCache = new MemoryCache(new MemoryCacheOptions());

        public static async Task<string> GetPublicKeyXmlAsync(Tenant tenant, string fundamentalsBaseUrl, CancellationToken token = default)
        {
            return await GetPublicKeyXmlAsync(tenant, fundamentalsBaseUrl, "AuthServicePublicKey", token);
        }

        public static async Task<RsaSecurityKey> GetPublicRsaKeyAsync(Tenant tenant, string fundamentalsBaseUrl, CancellationToken token = default)
        {
            var publicKeyXml = await GetPublicKeyXmlAsync(tenant, fundamentalsBaseUrl, token);
            FulcrumAssert.IsNotNullOrWhiteSpace(publicKeyXml);
            return CreateRsaSecurityKeyFromXmlString(publicKeyXml);
        }

        public static RsaSecurityKey CreateRsaSecurityKeyFromXmlString(string publicKeyXml)
        {
            using (var provider = new RSACryptoServiceProvider(RsaKeySizeInBits))
            {
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

        protected static async Task<string> GetPublicKeyXmlAsync(Tenant tenant, string fundamentalsBaseUrl, string type, CancellationToken token)
        {
            var key = $"{type}|{tenant}";
            var publicKeyXml = PublicKeyCache.Get<string>(key);
            if (!string.IsNullOrWhiteSpace(publicKeyXml)) return publicKeyXml;

            var url = $"{fundamentalsBaseUrl}/api/v2/Organizations/{tenant.Organization}/Environments/{tenant.Environment}/Tokens/{type}";
            try
            {
                var response = await HttpClient.GetAsync(url, token);
                if (!response.IsSuccessStatusCode) throw new Exception($"Response code {response.StatusCode}");
                var result = await response.Content.ReadAsStringAsync();
                var publicKey = JsonConvert.DeserializeObject<string>(result);
                PublicKeyCache.Set(key, publicKey, DateTimeOffset.MaxValue);
                return publicKey;
            }
            catch (Exception e)
            {
                Log.LogError($"Failed fetching '{type}' public key from '{url}'", e);
            }

            return null;
        }

        public static bool HasRole(string role, ClaimsPrincipal principal)
        {
            InternalContract.RequireNotNull(principal, nameof(principal));

            return Validation.HasRole(role, principal);
        }

        public static string GetClaimValue(string type, ClaimsPrincipal principal)
        {
            InternalContract.RequireNotNull(principal, nameof(principal));

            return Validation.GetClaimValue(type, principal);
        }

        public async Task<AuthenticationToken> RequestAndCacheJwtTokenAsync(IAuthenticationCredentials credentials, TimeSpan lifeSpan, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientId, nameof(credentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientSecret, nameof(credentials.ClientSecret));
            InternalContract.RequireNotNull(lifeSpan, nameof(lifeSpan));

            var token = await RequestJwtTokenAsync(credentials, lifeSpan, cancellationToken);
            FulcrumAssert.IsNotNull(token, $"{Namespace}: A9CC803F-A45A-4F93-AF4E-BA455E29893D", $"Failed to get a token for client {ServiceDescription(credentials.ClientId)}.");

            _tokenCache.AddOrUpdate(_type, credentials, token);
            return token;
        }

        private async Task<AuthenticationToken> RequestJwtTokenAsync(IAuthenticationCredentials credentials, TimeSpan lifeSpan, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientId, nameof(credentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientSecret, nameof(credentials.ClientSecret));
            InternalContract.RequireNotNull(lifeSpan, nameof(lifeSpan));

            var relativeUrl = $"Tokens/?hoursToLive={lifeSpan.TotalHours}";
            if (_legacyIssuer) relativeUrl += "&legacyIssuer=true";
            var uri = new Uri(ServiceUri, relativeUrl);
            string data = null;
            try
            {
                var serializedCredentials = JsonConvert.SerializeObject(credentials);
                var request = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Content = new StringContent(serializedCredentials, Encoding.UTF8, "application/json")
                };

                // TODO: Remove Basic Auth when Fundamentals with new auth is released in all environments
                var basicCredentialsAsBase64 = Base64Encode("user:pwd");
                request.Headers.Add("Authorization", $"Basic {basicCredentialsAsBase64}");
                request.Headers.Add("User-Agent", $"{credentials.ClientId}_{Tenant.Organization}_{Tenant.Environment}");

                var response = await HttpClient.SendAsync(request, cancellationToken);
                data = response.Content == null ? null : await response.Content.ReadAsStringAsync();
                if (data == null) return null;
                return JsonConvert.DeserializeObject<AuthenticationToken>(data);
            }
            catch (Exception e)
            {
                Log.LogError($"Error creating token on '{uri}' for client '{credentials.ClientId}'. Response: '{data}'", e);
                throw;
            }
        }

        internal string ServiceDescription(string clientId) => $"POST {ServiceUri} ClientId: {clientId}";

        internal string ServiceDescription(Uri uri, string clientId) => $"POST {uri} ClientId: {clientId}";

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}