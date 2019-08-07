using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Authentication
{
    /// <inheritdoc />
    public class PublicKeyLogic : IPublicKeyService
    {
        private readonly AuthenticationManager _authenticationManager;
        private static string _publicKey;

        /// <inheritdoc />
        public PublicKeyLogic(AuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        /// <inheritdoc />
        public async Task<string> GetPublicRsaKeyAsXmlAsync(CancellationToken token = default(CancellationToken))
        {
            if (_publicKey != null) return _publicKey;
            var semaphore = new NexusAsyncSemaphore();
            var publicKey = await semaphore.ExecuteAsync(GetKeyFromNexus, token);
            FulcrumAssert.IsNotNullOrWhiteSpace(publicKey);
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
