using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk.RestClients
{
    /// <inheritdoc cref="IRequestService" />
    public class RequestRestClient : RestClient, IRequestService
    {
        private readonly IHttpSender _httpSender;

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Requests"))
        {
            _httpSender = httpSender;
        }

        /// <inheritdoc />
        public Task<string> CreateAsync(HttpRequestCreate request, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(request, nameof(request));
            InternalContract.RequireValidated(request, nameof(request));

            return PostAsync<string, HttpRequestCreate>("", request, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task RetryAsync(string requestId, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(requestId, nameof(requestId));

            var relativeUrl = $"{WebUtility.UrlEncode(requestId)}/Retry";
            return PostNoResponseContentAsync(relativeUrl, null, cancellationToken);
        }

        /// <inheritdoc />
        public RequestResponseEndpoints GetEndpoints(string requestId)
        {
            return new RequestResponseEndpoints
            {
                PollingUrl = $"/Requests/{WebUtility.UrlEncode(requestId)}/Response",
                RegisterCallbackUrl = null
            };
        }
    }
}