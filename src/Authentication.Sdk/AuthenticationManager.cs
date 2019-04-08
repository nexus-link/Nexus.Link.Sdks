using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
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
    public class AuthenticationManager
    {
        private static readonly string Namespace = typeof(AuthenticationManager).Namespace;
        private static readonly HttpClient HttpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());
        private static readonly Dictionary<string, TokenCache> TokenCaches = new Dictionary<string, TokenCache>();
        private readonly TokenCache _tokenCache;
        public Tenant Tenant { get; }
        public Uri ServiceUri { get; }
        private readonly string _type;

        public static string LegacyIssuer => Validation.LegacyIssuer;
        public static string LegacyAudience => Validation.LegacyAudience;
        public static string NexusIssuer => Validation.NexusIssuer;
        public static string AuthServiceIssuer => Validation.AuthServiceIssuer;


        [Obsolete("No use for serviceCredentials anymore", true)]
        // ReSharper disable once UnusedParameter.Local
        public AuthenticationManager(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials) : this(tenant, serviceUri)
        {
        }

        public AuthenticationManager(Tenant tenant, string serviceBaseUrl) : this("AuthService", tenant, serviceBaseUrl, $"api/v1/{tenant.Organization}/{tenant.Environment}/Authentication/")
        {
        }

        protected AuthenticationManager(string type, Tenant tenant, string serviceBaseUrl, string path)
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
            if (!TokenCaches.TryGetValue(cacheKey, out _tokenCache))
            {
                _tokenCache = new TokenCache();
                TokenCaches[cacheKey] = _tokenCache;
            }

            if (!serviceBaseUrl.EndsWith("/")) serviceBaseUrl += "/";
            if (!path.EndsWith("/")) path += "/";
            var baseUri = new Uri(serviceBaseUrl);
            ServiceUri = new Uri(baseUri, path);
        }

        [Obsolete("No use for serviceCredentials anymore", true)]
        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            return await GetJwtTokenAsync(tenant, serviceUri, tokenCredentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNull(tokenCredentials, nameof(tokenCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(minimumExpirationSpan, nameof(minimumExpirationSpan));
            InternalContract.RequireNotNull(maximumExpirationSpan, nameof(maximumExpirationSpan));

            var authentication = new AuthenticationManager(tenant, serviceUri);
            return await authentication.GetJwtTokenAsync(tokenCredentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        [Obsolete("No use for serviceCredentials anymore", true)]
        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan)
        {
            return await GetJwtTokenAsync(tenant, serviceUri, tokenCredentials, minimumExpirationSpan);
        }

        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan)
        {
            return await GetJwtTokenAsync(tenant, serviceUri, tokenCredentials, minimumExpirationSpan, TimeSpan.FromHours(24));
        }

        [Obsolete("No use for serviceCredentials anymore", true)]
        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials)
        {
            return await GetJwtTokenAsync(tenant, serviceUri, tokenCredentials);
        }

        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials)
        {
            return await GetJwtTokenAsync(tenant, serviceUri, tokenCredentials, TimeSpan.FromHours(1), TimeSpan.FromHours(24));
        }

        [Obsolete("No use for serviceCredentials anymore", true)]
        public static ITokenRefresherWithServiceClient CreateTokenRefresher(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            return CreateTokenRefresher(tenant, serviceUri, tokenCredentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        public static ITokenRefresherWithServiceClient CreateTokenRefresher(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));

            var authentication = new AuthenticationManager(tenant, serviceUri);
            return authentication.CreateTokenRefresher(tokenCredentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        [Obsolete("No use for serviceCredentials anymore", true)]
        public static ITokenRefresherWithServiceClient CreateTokenRefresher(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials)
        {
            return CreateTokenRefresher(tenant, serviceUri, tokenCredentials);
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

        public async Task<AuthenticationToken> GetJwtTokenAsync(IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(tokenCredentials, nameof(tokenCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientId, nameof(tokenCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientSecret, nameof(tokenCredentials.ClientSecret));
            InternalContract.RequireNotNull(minimumExpirationSpan, nameof(minimumExpirationSpan));
            InternalContract.RequireNotNull(maximumExpirationSpan, nameof(maximumExpirationSpan));

            var token = _tokenCache.Get(_type, tokenCredentials, minimumExpirationSpan);
            if (token != null) return token;
            token = await RequestAndCacheJwtTokenAsync(tokenCredentials, maximumExpirationSpan);
            return token;
        }

        public async Task<AuthenticationToken> GetJwtTokenAsync(IAuthenticationCredentials tokenCredentials)
        {
            InternalContract.RequireNotNull(tokenCredentials, nameof(tokenCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientId, nameof(tokenCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientSecret, nameof(tokenCredentials.ClientSecret));

            return await GetJwtTokenAsync(tokenCredentials, TimeSpan.FromHours(1), TimeSpan.FromHours(24));
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
        public static ClaimsPrincipal ValidateToken(string token, string publicKey, string issuer)
        {
            InternalContract.RequireNotNullOrWhiteSpace(token, nameof(token));
            InternalContract.RequireNotNullOrWhiteSpace(publicKey, nameof(publicKey));

            return Validation.ValidateToken(token, publicKey, issuer);
        }

        public static JwtSecurityToken ReadTokenNotValidating(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            return jsonToken;
        }

        private static readonly MemoryCache PublicKeyCache = new MemoryCache(new MemoryCacheOptions());

        public static async Task<string> GetPublicKeyXmlAsync(Tenant tenant, string fundamentalsBaseUrl)
        {
            return await GetPublicKeyXmlAsync(tenant, fundamentalsBaseUrl, "AuthServicePublicKey");
        }

        protected static async Task<string> GetPublicKeyXmlAsync(Tenant tenant, string fundamentalsBaseUrl, string type)
        {
            var key = $"{type}|{tenant}";
            var publicKeyXml = PublicKeyCache.Get<string>(key);
            if (!string.IsNullOrWhiteSpace(publicKeyXml)) return publicKeyXml;

            var url = $"{fundamentalsBaseUrl}/api/v2/Organizations/{tenant.Organization}/Environments/{tenant.Environment}/Tokens/{type}";
            try
            {
                var response = await HttpClient.GetAsync(url);
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

        internal async Task<AuthenticationToken> RequestAndCacheJwtTokenAsync(IAuthenticationCredentials credentials, TimeSpan lifeSpan)
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientId, nameof(credentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientSecret, nameof(credentials.ClientSecret));
            InternalContract.RequireNotNull(lifeSpan, nameof(lifeSpan));

            var token = await RequestJwtTokenAsync(credentials, lifeSpan);
            FulcrumAssert.IsNotNull(token, $"{Namespace}: A9CC803F-A45A-4F93-AF4E-BA455E29893D", $"Failed to get a token for client {ServiceDescription(credentials.ClientId)}.");

            _tokenCache.AddOrUpdate(_type, credentials, token);
            return token;
        }

        private async Task<AuthenticationToken> RequestJwtTokenAsync(IAuthenticationCredentials credentials, TimeSpan lifeSpan)
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientId, nameof(credentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientSecret, nameof(credentials.ClientSecret));
            InternalContract.RequireNotNull(lifeSpan, nameof(lifeSpan));

            var uri = new Uri(ServiceUri, $"Tokens/?hoursToLive={lifeSpan.TotalHours}");
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

                var response = await HttpClient.SendAsync(request);
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