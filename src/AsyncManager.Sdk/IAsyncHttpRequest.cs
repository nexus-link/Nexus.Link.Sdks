using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;

namespace Nexus.Link.AsyncManager.Sdk
{
    /// <summary>
    /// Methods for adding things to a request.
    /// </summary>
    public interface IAsyncHttpRequest
    {
        /// <summary>
        /// Add one header to the <see cref="HttpRequestCreate.Headers"/> property.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        /// <param name="values">The values for the header.</param>
        /// <remarks>If a header with the same name already exists, the values are appended to the existing header values.</remarks>
        AsyncHttpRequest AddHeader(string name, string[] values);

        /// <summary>
        /// Add one header to the <see cref="HttpRequestCreate.Headers"/> property.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        /// <param name="value">The value for the header.</param>
        /// <remarks>If a header with the same name already exists, the values are appended to the existing header values.</remarks>
        AsyncHttpRequest AddHeader(string name, string value);

        /// <summary>
        /// Add a number of headers to the <see cref="HttpRequestCreate.Headers"/> property.
        /// </summary>
        /// <param name="headers">The headers to add.</param>
        /// <remarks>If a header with the same name already exists, the values are appended to the existing header values.</remarks>
        AsyncHttpRequest AddHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers);

        /// <summary>
        /// Set the <see cref="HttpRequestCreate.Content"/> property to the data provided in <paramref name="content"/> with
        /// content type set to <paramref name="contentType"/>.
        /// </summary>
        AsyncHttpRequest SetContent(string content, string contentType);

        /// <summary>
        /// Set the <see cref="HttpRequestCreate.Content"/> property to the data provided in <paramref name="content"/> with
        /// content type set to "application/json".
        /// </summary>
        AsyncHttpRequest SetContent(JToken content);

        /// <summary>
        /// Set the <see cref="HttpRequestCreate.Content"/> property to the data provided in <paramref name="content"/> with
        /// content type set to "application/json".
        /// </summary>
        AsyncHttpRequest SetContentAsJson(object content);

        /// <summary>
        /// Set a limit for when the request must have been sent to the server in the <see cref="RequestMetadata.SendBefore"/> property.
        /// </summary>
        /// <param name="before">The absolute time the request must have been sent to the server.</param>
        [Obsolete("Please use SetSendBefore. Obsolete since 2021-01-07.")]
        AsyncHttpRequest SetExecuteBefore(DateTimeOffset before);

        /// <summary>
        /// Set a limit for when the request must have been sent to the server in the <see cref="RequestMetadata.SendBefore"/> property.
        /// </summary>
        /// <param name="before">A relative time from now when the request must have been sent to the server.</param>
        [Obsolete("Please use SetSendBefore. Obsolete since 2021-01-07.")]
        AsyncHttpRequest SetExecuteBefore(TimeSpan before);

        /// <summary>
        /// Set the earliest time that the request can be sent to the server in the <see cref="RequestMetadata.SendAfter"/> property.
        /// </summary>
        /// <param name="after">Time limit as absolute time.</param>
        [Obsolete("Please use SetSendAfter. Obsolete since 2021-01-07.")]
        AsyncHttpRequest SetExecuteAfter(DateTimeOffset after);

        /// <summary>
        /// Set the earliest time that the request can be sent to the server in the <see cref="RequestMetadata.SendAfter"/> property.
        /// </summary>
        /// <param name="after">A relative time from now.</param>
        [Obsolete("Please use SetSendAfter. Obsolete since 2021-01-07.")]
        AsyncHttpRequest SetExecuteAfter(TimeSpan after);

        /// <summary>
        /// Set a limit for when the request must have been sent to the server in the <see cref="RequestMetadata.SendBefore"/> property.
        /// </summary>
        /// <param name="before">The absolute time the request must have been sent to the server.</param>
        AsyncHttpRequest SetSendBefore(DateTimeOffset before);

        /// <summary>
        /// Set a limit for when the request must have been sent to the server in the <see cref="RequestMetadata.SendBefore"/> property.
        /// </summary>
        /// <param name="before">A relative time from now when the request must have been sent to the server.</param>
        AsyncHttpRequest SetSendBefore(TimeSpan before);

        /// <summary>
        /// Set the earliest time that the request can be sent to the server in the <see cref="RequestMetadata.SendAfter"/> property.
        /// </summary>
        /// <param name="after">Time limit as absolute time.</param>
        AsyncHttpRequest SetSendAfter(DateTimeOffset after);

        /// <summary>
        /// Set the earliest time that the request can be sent to the server in the <see cref="RequestMetadata.SendAfter"/> property.
        /// </summary>
        /// <param name="after">A relative time from now.</param>
        AsyncHttpRequest SetSendAfter(TimeSpan after);

        /// <summary>
        /// Set the request to be called when a response is available.
        /// </summary>
        /// <param name="waitingRequestId">A request of postpone type (typically a workflow) that want to be called again when the response is available.</param>
        AsyncHttpRequest SetWaitingRequestId(string waitingRequestId);

        /// <summary>
        /// Set the callback URL to <paramref name="url"/>.
        /// </summary>
        AsyncHttpRequest SetCallbackUrl(string url);

        /// <summary>
        /// Set the callback URL to absolute path of <paramref name="uri"/>.
        /// </summary>
        AsyncHttpRequest SetCallbackUrl(Uri uri);

        /// <summary>
        /// Add one header to the <see cref="RequestMetadata.Callback"/> <see cref="RequestCallback.Headers"/> property.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        /// <param name="values">The values for the header.</param>
        /// <remarks>If a header with the same name already exists, the values are appended to the existing header values.</remarks>
        AsyncHttpRequest AddCallbackHeader(string name, string[] values);

        /// <summary>
        /// Add one header to the <see cref="RequestMetadata.Callback"/> <see cref="RequestCallback.Headers"/> property.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        /// <param name="value">The value for the header.</param>
        /// <remarks>If a header with the same name already exists, the values are appended to the existing header values.</remarks>
        AsyncHttpRequest AddCallbackHeader(string name, string value);

        /// <summary>
        /// Send the request to the async request manager.
        /// </summary>
        /// <param name="cancellationToken">Token for cancelling a call</param>
        /// <returns>The Guid for the accepted request.</returns>
        Task<string> SendAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the callback context to <paramref name="context"/>.
        /// </summary>
        AsyncHttpRequest SetCallbackContext(string context);

        /// <summary>
        /// Set the callback context to <paramref name="context"/>.
        /// </summary>
        AsyncHttpRequest SetCallbackContextAsJson(object context);
    }
}