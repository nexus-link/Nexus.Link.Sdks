using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Controllers.Capabilities.Integration.Authentication
{
    /// <summary>
    /// Service implementation of <see cref="ITokenService"/>
    /// </summary>
    [ApiController]
    [Area("Authentication")]
    [Route("api/Integration/v1/[area]/v1/Tokens")]
    public class NexusTokensController : ControllerBase, ITokenService
    {
        protected readonly IAuthenticationCapability Capability;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capability">The logic layer</param>
        public NexusTokensController(IAuthenticationCapability capability)
        {
            Capability = capability;
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("")]
        public Task<AuthenticationToken> ObtainAccessTokenAsync(AuthenticationCredentials credentials, CancellationToken token = default)
        {
            ServiceContract.RequireNotNull(credentials, nameof(credentials));
            ServiceContract.RequireValidated(credentials, nameof(credentials));
            return Capability.TokenService.ObtainAccessTokenAsync(credentials, token);
        }

        /// <inheritdoc />
        [HttpGet("PublicKeyAsXml")]
        public Task<string> GetPublicRsaKeyAsXmlAsync(CancellationToken token = default)
        {
            return Capability.TokenService.GetPublicRsaKeyAsXmlAsync(token);
        }
    }
}
