using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Authentication
{
    /// <inheritdoc />
    public class TokenLogic : ITokenService
    {
        private static AuthenticationManager _authenticationManager;

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
    }
}
