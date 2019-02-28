using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Authentication.Sdk.Logic;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk
{
    public class AuthenticationManager
    {
        private static readonly string Namespace = typeof(AuthenticationManager).Namespace;
        private static readonly HttpClient HttpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());
        private static readonly Dictionary<string, TokenCache> TokenCaches = new Dictionary<string, TokenCache>();
        private readonly TokenCache _tokenCache;
        public Tenant Tenant { get; }
        public Uri ServiceUri { get; }
        internal AuthenticationCredentials ServiceCredentials { get; }
        public static string Issuer => Validation.Issuer;
        public static string Audience => Validation.Audience;
        public static byte[] PublicSecurityKeyExponent => Validation.PublicSecurityKeyExponent;
        public static byte[] PublicSecurityKeyModulus => Validation.PublicSecurityKeyModulus;

           
        public AuthenticationManager(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(serviceCredentials, nameof(serviceCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientId, nameof(serviceCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientSecret, nameof(serviceCredentials.ClientSecret));

            Tenant = tenant;
            var cacheKey = $"{tenant.Organization}{tenant.Environment}";
            if (!TokenCaches.TryGetValue(cacheKey, out _tokenCache))
            {
                _tokenCache = new TokenCache();
                TokenCaches[cacheKey] = _tokenCache;
            }

            if (!serviceUri.EndsWith("/")) serviceUri += "/";
            var baseUri = new Uri(serviceUri);
            ServiceUri = new Uri(baseUri, $"api/v1/{tenant.Organization}/{tenant.Environment}/");
            ServiceCredentials = new AuthenticationCredentials
            {
                ClientId = serviceCredentials.ClientId,
                ClientSecret = serviceCredentials.ClientSecret
            };
        }

        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan,
            TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(serviceCredentials, nameof(serviceCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientId, nameof(serviceCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientSecret, nameof(serviceCredentials.ClientSecret));
            InternalContract.RequireNotNull(minimumExpirationSpan, nameof(minimumExpirationSpan));
            InternalContract.RequireNotNull(maximumExpirationSpan, nameof(maximumExpirationSpan));

            var authentication = new AuthenticationManager(tenant, serviceUri, serviceCredentials);
            return await authentication.GetJwtTokenAsync(tokenCredentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(serviceCredentials, nameof(serviceCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientId, nameof(serviceCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientSecret, nameof(serviceCredentials.ClientSecret));
            InternalContract.RequireNotNull(minimumExpirationSpan, nameof(minimumExpirationSpan));

            return await GetJwtTokenAsync(tenant, serviceUri, serviceCredentials, tokenCredentials, minimumExpirationSpan,
                TimeSpan.FromHours(24));
        }

        public static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(serviceCredentials, nameof(serviceCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientId, nameof(serviceCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientSecret, nameof(serviceCredentials.ClientSecret));

            return await GetJwtTokenAsync(tenant, serviceUri, serviceCredentials, tokenCredentials, TimeSpan.FromHours(1),
                TimeSpan.FromHours(24));
        }
        public static ITokenRefresherWithServiceClient CreateTokenRefresher(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(serviceCredentials, nameof(serviceCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientId, nameof(serviceCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientSecret, nameof(serviceCredentials.ClientSecret));

            var authentication = new AuthenticationManager(tenant, serviceUri, serviceCredentials);
            return authentication.CreateTokenRefresher(tokenCredentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        public static ITokenRefresherWithServiceClient CreateTokenRefresher(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials, IAuthenticationCredentials tokenCredentials)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(serviceCredentials, nameof(serviceCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientId, nameof(serviceCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(serviceCredentials.ClientSecret, nameof(serviceCredentials.ClientSecret));

            var authentication = new AuthenticationManager(tenant, serviceUri, serviceCredentials);
            return authentication.CreateTokenRefresher(tokenCredentials);
        }

        public async Task<AuthenticationToken> GetJwtTokenAsync(IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(tokenCredentials, nameof(tokenCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientId, nameof(tokenCredentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(tokenCredentials.ClientSecret, nameof(tokenCredentials.ClientSecret));
            InternalContract.RequireNotNull(minimumExpirationSpan, nameof(minimumExpirationSpan));
            InternalContract.RequireNotNull(maximumExpirationSpan, nameof(maximumExpirationSpan));

            var token = _tokenCache.Get(tokenCredentials, minimumExpirationSpan);
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

        public static ClaimsPrincipal ValidateToken(string token)
        {
            InternalContract.RequireNotNullOrWhiteSpace(token, nameof(token));

            return Validation.ValidateToken(token);
        }

        public static bool HasRole(AuthenticationRoleEnum role, ClaimsPrincipal principal)
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

            _tokenCache.AddOrUpdate(credentials, token);
            return token;
        }

        private async Task<AuthenticationToken> RequestJwtTokenAsync(IAuthenticationCredentials credentials, TimeSpan lifeSpan)
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientId, nameof(credentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientSecret, nameof(credentials.ClientSecret));
            InternalContract.RequireNotNull(lifeSpan, nameof(lifeSpan));

            var serializedCredentials = JsonConvert.SerializeObject(credentials);
            var uri = new Uri(ServiceUri, $"Tokens/?hoursToLive={lifeSpan.TotalHours}");
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(serializedCredentials, Encoding.UTF8, "application/json")
            };

            var basicCredentialsAsBase64 = BasicCredentialsAsBase64;
            request.Headers.Add("Authorization", $"Basic {basicCredentialsAsBase64}");
            request.Headers.Add("User-Agent", $"{credentials.ClientId}_{Tenant.Organization}_{Tenant.Environment}");

            var response = await HttpClient.SendAsync(request);
            var data = response.Content == null ? null : await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AuthenticationToken>(data);
        }

        internal string ServiceDescription(string clientId) => $"POST {ServiceUri} ClientId: {clientId}";

        internal string ServiceDescription(Uri uri, string clientId) => $"POST {uri} ClientId: {clientId}";

        private string BasicCredentialsAsBase64 => Base64Encode($"{ServiceCredentials.ClientId}:{ServiceCredentials.ClientSecret}");

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}