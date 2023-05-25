using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;

namespace Nexus.Link.AsyncManager.Sdk
{
    /// <summary>
    /// A client for using an async request management capability.
    /// </summary>
    public interface IAsyncRequestClient
    {
        /// <summary>
        /// Create a <see cref="AsyncHttpRequest"/> that has convenience methods for setting up a <see cref="HttpRequestCreate"/>.
        /// </summary>
        /// <param name="method">The HTTP method for the request.</param>
        /// <param name="url">The URL where the request should be sent.</param>
        /// <param name="priority">The priority for the request in the range [0.0, 1.0] where 1.0 is highest priority.</param>
        IAsyncHttpRequest CreateRequest(HttpMethod method, string url, double priority);

        /// <summary>
        /// Get the final response from the async request manager, i.e. ignore intermediate responses that are not considered to be the final response.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="cancellationToken">Token for cancelling a call</param>
        /// <returns>The final response or null if the final response is not yet available.</returns>
        Task<AsyncHttpResponse> GetFinalResponseAsync(string requestId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send one asynchronous request to the async request manager.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Token for cancelling a call</param>
        /// <returns>The request id for the posted request.</returns>
        Task<string> SendRequestAsync(AsyncHttpRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send one asynchronous request to the async request manager.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Token for cancelling a call</param>
        /// <returns>The request id for the posted request.</returns>
        Task<string> SendRequestAsync(IAsyncHttpRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the authentication of a <see cref="HttpRequest"/>,
        /// typically used by the calling client when receiving AM callback with 401 response.
        /// </summary>
        /// <param name="requestId">The identification of <see cref="HttpRequest"/></param>
        /// <param name="input">The new authentication</param>
        /// <param name="cancellationToken">Token for cancelling a call</param>
        /// <returns></returns>
        [Obsolete("Please use CreateRequestCopyWithNewAuthenticationAsync. Please note that it returns a new request id for the copy that will be used for retrying. Obsolete from 2023-05-17.", true)]
        Task RetryRequestWithNewAuthenticationAsync(string requestId, RequestAuthentication input, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update the authentication of a <see cref="HttpRequest"/>,
        /// typically used by the calling client when receving AM callback with 401 response.
        /// </summary>
        /// <param name="requestId">The identification of <see cref="HttpRequest"/></param>
        /// <param name="input">The new authentication</param>
        /// <param name="cancellationToken">Token for cancelling a call</param>
        /// <returns></returns>
        Task<string> CreateRequestCopyWithNewAuthenticationAsync(string requestId, RequestAuthentication input, CancellationToken cancellationToken = default);
    }
}