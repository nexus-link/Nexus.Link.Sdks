using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Controllers.Capabilities.Integration.Authentication
{
    /// <summary>
    /// Service implementation of <see cref="IPublicKeyService"/>
    /// </summary>
    public abstract class PublicKeysControllerBase : ControllerBase, IPublicKeyService
    {
        /// <summary>
        /// The surrounding capability
        /// </summary>
        protected  IAuthenticationCapability Capability { get; }

        /// <inheritdoc />
        protected PublicKeysControllerBase(IAuthenticationCapability capability)
        {
            Capability = capability;
        }

        /// <inheritdoc />
        [HttpGet("")]
        public Task<string> GetPublicRsaKeyAsXmlAsync(CancellationToken token = default(CancellationToken))
        {
            return Capability.PublicKeyService.GetPublicRsaKeyAsXmlAsync(token);
        }
    }
}
