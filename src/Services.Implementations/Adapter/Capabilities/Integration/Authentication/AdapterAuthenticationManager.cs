using System;
using System.Text;
using System.Threading.Tasks;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Authentication.Sdk.Logic;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    internal class AdapterAuthenticationManager : IJwtTokenHandler
    {
        private readonly ITokenService _tokenService;
        private readonly TokenCache _tokenCache;
        private readonly string _type;

        public AdapterAuthenticationManager(ITokenService tokenService)
        {
            _tokenService = tokenService;
            _type = nameof(AdapterAuthenticationManager);
            _tokenCache = new TokenCache();
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

        public async Task<AuthenticationToken> RequestAndCacheJwtTokenAsync(IAuthenticationCredentials credentials, TimeSpan lifeSpan)
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientId, nameof(credentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientSecret, nameof(credentials.ClientSecret));
            InternalContract.RequireNotNull(lifeSpan, nameof(lifeSpan));

            var token = await RequestJwtTokenAsync(credentials, lifeSpan);
            FulcrumAssert.IsNotNull(token, CodeLocation.AsString(), $"Failed to get a token for client {credentials.ClientId}.");

            _tokenCache.AddOrUpdate(_type, credentials, token);
            return token;
        }

        private async Task<AuthenticationToken> RequestJwtTokenAsync(IAuthenticationCredentials credentials, TimeSpan lifeSpan)
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientId, nameof(credentials.ClientId));
            InternalContract.RequireNotNullOrWhiteSpace(credentials.ClientSecret, nameof(credentials.ClientSecret));
            InternalContract.RequireNotNull(lifeSpan, nameof(lifeSpan));

            return await _tokenService.ObtainAccessTokenAsync(credentials as AuthenticationCredentials);
        }
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
