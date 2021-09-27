using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk
{
    /// <summary>
    /// A client for using an async request management capability.
    /// </summary>
    public class AsyncRequestClient : IAsyncRequestClient
    {
        private readonly IHttpSender _httpSender;
        private readonly Tenant _tenant;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="httpSender">The sender that will be used for posting the request to the async request manager.</param>
        /// 
        public AsyncRequestClient(Tenant tenant, IHttpSender httpSender)
        {
            InternalContract.RequireNotNull(httpSender, nameof(httpSender));

            _httpSender = httpSender;
            _tenant = tenant;
        }

        /// <inheritdoc />
        public AsyncHttpRequest CreateRequest(HttpMethod method, string url, double priority)
        {
            return new AsyncHttpRequest(this, method, url, priority);
        }

        /// <inheritdoc />
        public async Task<string> SendRequestAsync(AsyncHttpRequest request, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(request, nameof(request));
            InternalContract.RequireValidated(request, nameof(request));

            var result = await _httpSender.SendRequestAsync<string, AsyncHttpRequest>(HttpMethod.Post,
                $"api/v1/Tenant/{_tenant.Organization}/{_tenant.Environment}/Requests", request, null, cancellationToken);
            //Should we really do asserts here? If result.IsSuccessStatusCode is false we'll throw a FulcrumAssertionFailedException with the message 'Expected value to be true'. Feels bad man.

            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsNotNull(result.Response, CodeLocation.AsString());
            if (result!.Response.IsSuccessStatusCode != true)
            {
                throw new FulcrumResourceException($"Expected successful statusCode or the httpSender to throw an exception, but received HTTP status {result.Response.StatusCode}.\r" +
                                                   $"We recommend that you use {typeof(ThrowFulcrumExceptionOnFail).FullName} as a handler in your {nameof(IHttpSender)}; it will convert failed HTTP requests to exceptions.");
            }

            return result.Body;
        }

        /// <summary>
        /// Get the final response if it exists, otherwise null.
        /// </summary>
        public async Task<AsyncHttpResponse> GetFinalResponseAsync(string requestId, CancellationToken cancellationToken = default)
        {
            var result = await _httpSender.SendRequestAsync<AsyncHttpResponse, object>(HttpMethod.Get, $"api/v1/Tenant/{_tenant.Organization}/{_tenant.Environment}/Requests/{requestId}/Response", cancellationToken: cancellationToken);
            var response = result.Body;
            if (response == null) return null;
            FulcrumAssert.IsValidated(response, CodeLocation.AsString());
            return !response.HasCompleted ? null : response;
        }
    }
}