using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication
{
    // Service for getting the public key
    public interface IPublicKeyService
    {
        /// <summary>
        /// Get the public RSA key
        /// </summary>
        /// <param name="token"></param>
        /// <returns>XML-representation of the public rsa key</returns>
        Task<string> GetPublicRsaKeyAsXmlAsync(CancellationToken token = default(CancellationToken));
    }
}
