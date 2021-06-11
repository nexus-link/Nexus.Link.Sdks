using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk
{
    public interface IJwtTokenHandler
    {
        Task<AuthenticationToken> GetJwtTokenAsync(IAuthenticationCredentials tokenCredentials,
            TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan, CancellationToken cancellationToken = default);

        Task<AuthenticationToken> RequestAndCacheJwtTokenAsync(IAuthenticationCredentials credentials,
            TimeSpan lifeSpan, CancellationToken cancellationToken = default);
    }
}