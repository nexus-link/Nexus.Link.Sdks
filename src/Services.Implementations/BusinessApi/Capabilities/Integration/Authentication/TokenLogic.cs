using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Authentication
{
    /// <inheritdoc />
    public class TokenLogic : ITokenService
    {
        private static AuthenticationManager _authenticationManager;
        private readonly NexusAsyncSemaphore _semaphore = new NexusAsyncSemaphore();
        private string _publicKey;

        /// <inheritdoc />
        public TokenLogic(AuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        /// <inheritdoc />
        public Task<AuthenticationToken> ObtainAccessTokenAsync(AuthenticationCredentials credentials,
            CancellationToken token =
                default(CancellationToken))
        {
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireValidated(credentials, nameof(credentials));
            return _authenticationManager.GetJwtTokenAsync(credentials, TimeSpan.FromDays(7), TimeSpan.FromDays(31));
        }

        /// <inheritdoc />
        public async Task<string> GetPublicRsaKeyAsXmlAsync(CancellationToken token = default(CancellationToken))
        {
            if (_publicKey != null) return _publicKey;
            var publicKey = await _semaphore.ExecuteAsync(GetKeyFromNexus, token);
            FulcrumAssert.IsNotNullOrWhiteSpace(publicKey, CodeLocation.AsString());
            _publicKey = publicKey;
            return _publicKey;
        }

        private async Task<string> GetKeyFromNexus(CancellationToken token = default(CancellationToken))
        {
            var rsaPublicKey =
                await _authenticationManager.GetPublicKeyXmlAsync(token);
            return rsaPublicKey;
        }
    }
}
