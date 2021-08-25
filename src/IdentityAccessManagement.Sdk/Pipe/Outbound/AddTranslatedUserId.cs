using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Context;

namespace IdentityAccessManagement.Sdk.Pipe.Outbound
{
    /// <summary>
    /// Adds a Nexus Translated User Id header to all outgoing requests.
    /// </summary>
    public class AddTranslatedUserId : DelegatingHandler
    {
        private readonly IContextValueProvider _provider;
        private static string ContextKey = "TranslatedUserId";

        public AddTranslatedUserId()
        {
            _provider = new AsyncLocalContextValueProvider();
        }

        /// <summary>
        /// Adds a Nexus Translated User Id header to the request before sending it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var translatedUserId = _provider.GetValue<string>(ContextKey);

            if (!string.IsNullOrWhiteSpace(translatedUserId))
            {
                if (!request.Headers.TryGetValues("Nexus-Translated-User-Id", out IEnumerable<string> values))
                {
                    request.Headers.Add("Nexus-Translated-User-Id", translatedUserId);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}