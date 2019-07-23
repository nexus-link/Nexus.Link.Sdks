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
    public abstract class TokensControllerBase : ControllerBase, ITokenService
    {
        protected readonly IAuthenticationCapability Capability;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capability">The logic layer</param>
        protected TokensControllerBase(IAuthenticationCapability capability)
        {
            Capability = capability;
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("")]
        public Task<AuthenticationToken> ObtainAccessToken(AuthenticationCredentials credentials, CancellationToken token = default(CancellationToken))
        {
            ServiceContract.RequireNotNull(credentials, nameof(credentials));
            ServiceContract.RequireValidated(credentials, nameof(credentials));
            return Capability.TokenService.ObtainAccessToken(credentials, token);
        }
    }
}
