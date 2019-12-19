using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.Authentication
{
    /// <inheritdoc cref="ITokenService" />
    public class TokenRestService : RestClientBase, ITokenService
    {
        /// <inheritdoc cref="ITokenService" />
        public TokenRestService(IHttpSender httpSender)
        :base(httpSender)
        {
        }

        /// <inheritdoc />
        public Task<AuthenticationToken> ObtainAccessTokenAsync(AuthenticationCredentials credentials, CancellationToken token = default(CancellationToken))
        {
            return RestClient.PostAsync<AuthenticationToken, AuthenticationCredentials>("", credentials, null, token);
        }

        /// <inheritdoc />
        public Task<string> GetPublicRsaKeyAsXmlAsync(CancellationToken token = default(CancellationToken))
        {
            return RestClient.GetAsync<string>("PublicKeyAsXml", null, token);
        }
    }
}