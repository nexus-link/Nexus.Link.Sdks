using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Pipe.Outbound;

namespace IdentityAccessManagement.Sdk.Handlers
{
    public class TokenRefresher : ServiceClientCredentials
    {
        private static readonly HttpClient HttpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());
        
        private readonly ClientCredentialTokenrefresherOptions _options;
        private readonly IMemoryCache _cache;

        public TokenRefresher(ClientCredentialTokenrefresherOptions options, IMemoryCache cache)
        {
            InternalContract.RequireNotNull(options, nameof(options));
            InternalContract.RequireNotNullOrWhiteSpace(options.Authority, nameof(options.Authority));
            InternalContract.RequireNotNullOrWhiteSpace(options.ClientId, nameof(options.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(options.ClientSecret, nameof(options.ClientSecret));
            InternalContract.RequireNotNull(cache, nameof(cache));

            _options = options;
            _cache = cache;
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await GetTokenAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }

        private async Task<TokenResponse> GetTokenFromServerAsync(CancellationToken cancellationToken)
        {
            var tokenUrl = await GetTokenUrl(_options.Authority).ConfigureAwait(false);

            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = tokenUrl,
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Scope = _options.Scope
            };

            Log.LogInformation($"Trting to resolve bearer token by getting the access token from {tokenUrl} for client {_options.ClientId}.");
            var response = await HttpClient.RequestClientCredentialsTokenAsync(tokenRequest, cancellationToken);

            if (!response.IsError)
            {
                return response;
            }

            if (response.ErrorType == ResponseErrorType.Exception)
            {
                ExceptionDispatchInfo.Capture(response.Exception).Throw();
            }

            var error = response.ErrorType == ResponseErrorType.Http ? response.HttpErrorReason : $"Error: {response.Error}\nDescription: {response.ErrorDescription}";
            Log.LogError(error);
            throw new Exception(error);
        }

        public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            var cacheKey = $"IdentityAccessManagement.Sdk.Handlers.TokenRefresher:{_options.ClientId}";
            var token = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                var response = await GetTokenFromServerAsync(cancellationToken).ConfigureAwait(false);

                entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(response.ExpiresIn).AddSeconds(-20);
                return response.AccessToken;
            }).ConfigureAwait(false);

            return token;
        }

        private async Task<string> GetTokenUrl(string authority)
        {
            var discovery = new DiscoveryCache(authority, () => HttpClient);
            var disco = await discovery.GetAsync().ConfigureAwait(false);
            if (disco.IsError)
            {
                if (disco.Exception != null) ExceptionDispatchInfo.Capture(disco.Exception).Throw();
                throw new InvalidOperationException($"Failed to get token url. Details: {disco.Error ?? disco.HttpErrorReason}.");
            }

            return disco.TokenEndpoint;
        }
    }
}
