using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
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
        public Task RetryAsync(string requestId, CancellationToken cancellationToken = default)
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
                PollingUrl = $"{WebUtility.UrlEncode(requestId)}/Response",
                RegisterCallbackUrl = null
            };
        }

        /// <inheritdoc />
        [Obsolete("Please use CreateRequestCopyWithNewAuthenticationAsync. Please note that it returns a new request id for the copy that will be used for retrying. Obsolete from 2023-05-17.")]
        public async Task RetryRequestWithNewAuthenticationAsync(string requestId, RequestAuthentication newAuthentication, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(requestId, nameof(requestId));
            InternalContract.RequireNotNull(newAuthentication, nameof(newAuthentication));
            InternalContract.RequireValidated(newAuthentication, nameof(newAuthentication));

            await CreateRequestCopyWithNewAuthenticationAsync(requestId, newAuthentication, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> CreateRequestCopyWithNewAuthenticationAsync(string requestId, RequestAuthentication newAuthentication, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(requestId, nameof(requestId));
            InternalContract.RequireNotNull(newAuthentication, nameof(newAuthentication));
            InternalContract.RequireValidated(newAuthentication, nameof(newAuthentication));

            var relativeUrl = $"{WebUtility.UrlEncode(requestId)}/Authentication";
            var newRequestId = await PostAsync<string, RequestAuthentication>(relativeUrl, newAuthentication, cancellationToken: cancellationToken);
            return newRequestId;
        }
    }
}