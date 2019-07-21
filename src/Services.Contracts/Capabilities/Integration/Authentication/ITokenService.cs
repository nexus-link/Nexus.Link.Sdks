using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication
{
    /// <summary>
    /// Services for Tokens
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Obtain an access token
        /// </summary>
        /// <param name="credentials">Credentials to say who you are and to authenticate you.</param>
        /// <param name="token">Propagates notification that operations should be canceled.</param>
        Task<AuthenticationToken> ObtainAccessToken(AuthenticationCredentials credentials, CancellationToken token = default(CancellationToken));
    }
}
