using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk.RestClients
{
    /// <summary>
    /// A client for using an async request management capability.
    /// </summary>
    public class AsyncRequestClient : IAsyncRequestClient
    {
        private readonly IAsyncRequestMgmtCapability _capability;

        /// <summary>
        /// Constructor
        /// </summary> 
        public AsyncRequestClient(IAsyncRequestMgmtCapability capability)
        {
            InternalContract.RequireNotNull(capability, nameof(capability));
            _capability = capability;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("Use the constructor with an IAsyncRequestMgmtCapability parameter. Obsolete since 2021-09-29.")]
        public AsyncRequestClient(Tenant tenant, IHttpSender httpSender)
        {
            InternalContract.RequireNotNull(httpSender, nameof(httpSender));
            _capability = new AsyncRequestMgmtRestClients(httpSender);
        }

        /// <inheritdoc />
        public IAsyncHttpRequest CreateRequest(HttpMethod method, string url, double priority)
        {
            return new AsyncHttpRequest(this, method, url, priority);
        }

        /// <inheritdoc />
        public Task<string> SendRequestAsync(AsyncHttpRequest request, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(request, nameof(request));
            InternalContract.RequireValidated(request, nameof(request));

            return _capability.Request.CreateAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<string> SendRequestAsync(IAsyncHttpRequest request, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(request, nameof(request));
            InternalContract.RequireValidated(request, nameof(request));
            if (!(request is AsyncHttpRequest implementation))
            {
                throw new FulcrumContractException(
                    $"The {nameof(request)} parameter was expected to be implemented as a {nameof(AsyncHttpRequest)}" + 
                    $", but the type was {request.GetType().Name}");
            }

            return _capability.Request.CreateAsync(implementation, cancellationToken);
        }

        /// <inheritdoc />
        [Obsolete("Please use CreateRequestCopyWithNewAuthenticationAsync. Please note that it returns a new request id for the copy that will be used for retrying. Obsolete from 2023-05-17.", true)]
        public Task RetryRequestWithNewAuthenticationAsync(string requestId, RequestAuthentication input, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<string> CreateRequestCopyWithNewAuthenticationAsync(string requestId, RequestAuthentication input, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(requestId, nameof(requestId));

            return _capability.Request.CreateRequestCopyWithNewAuthenticationAsync(requestId, input, cancellationToken);
        }

        /// <summary>
        /// Get the final response if it exists, otherwise null.
        /// </summary>
        public async Task<AsyncHttpResponse> GetFinalResponseAsync(string requestId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(requestId, nameof(requestId));

            var result = await _capability.RequestResponse.ReadResponseAsync(requestId, cancellationToken);

            // TODO: This is a hairy cast from a base class to the super class.
            return JsonConvert.DeserializeObject<AsyncHttpResponse>(JsonConvert.SerializeObject(result));
        }
    }
}