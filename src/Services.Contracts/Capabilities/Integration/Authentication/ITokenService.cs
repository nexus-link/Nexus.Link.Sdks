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
        Task<AuthenticationToken> ObtainAccessTokenAsync(AuthenticationCredentials credentials, CancellationToken token = default);
        /// <summary>
        /// Get the public RSA key
        /// </summary>
        /// <param name="token"></param>
        /// <returns>XML-representation of the public rsa key</returns>
        Task<string> GetPublicRsaKeyAsXmlAsync(CancellationToken token = default);
    }
}
