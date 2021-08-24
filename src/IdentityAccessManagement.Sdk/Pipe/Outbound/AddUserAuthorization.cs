using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Context;

namespace IdentityAccessManagement.Sdk.Pipe.Outbound
{
    /// <summary>
    /// Adds a Nexus Iam User Authorization header to all outgoing requests.
    /// </summary>
    public class AddUserAuthorization : DelegatingHandler
    {
        private readonly IContextValueProvider _provider;
        private static string ContextKey = "UserAuthorization";

        public AddUserAuthorization()
        {
            _provider = new AsyncLocalContextValueProvider();
        }

        /// <summary>
        /// Adds a Nexus Iam User Authorization header to the request before sending it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var userAuthorization = _provider.GetValue<string>(ContextKey);

            if (!string.IsNullOrWhiteSpace(userAuthorization))
            {
                if (!request.Headers.TryGetValues("Nexus-User-Authorization", out IEnumerable<string> values))
                {
                    request.Headers.Add("Nexus-User-Authorization", userAuthorization);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}