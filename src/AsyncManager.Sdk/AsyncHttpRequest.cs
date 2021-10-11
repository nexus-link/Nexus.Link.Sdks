using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.AsyncManager.Sdk
{
    /// <summary>
    /// Keeps all the information about an asynchronous request
    /// </summary>
    public class AsyncHttpRequest : HttpRequestCreate, IAsyncHttpRequest
    {
        private readonly IAsyncRequestClient _asyncRequestClient;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="asyncRequestClient">The client to use for when sending the request to the async request manager.</param>
        /// <param name="method">The HTTP method for the request.</param>
        /// <param name="url">The URL where the request should be sent.</param>
        /// <param name="priority">The priority for the request in the range [0.0, 1.0] where 1.0 is highest priority.</param>
        protected internal AsyncHttpRequest(IAsyncRequestClient asyncRequestClient, HttpMethod method, string url, double priority)
        {
            InternalContract.RequireNotNull(asyncRequestClient, nameof(asyncRequestClient));
            InternalContract.RequireNotNullOrWhiteSpace(url, nameof(url));
            InternalContract.Require(HttpHelper.IsValidUri(url), $"The parameter {nameof(url)} must be a valid URL: {url}");
            InternalContract.RequireGreaterThanOrEqualTo(0.0, priority, nameof(priority));
            InternalContract.RequireLessThanOrEqualTo(1.0, priority, nameof(priority));

            _asyncRequestClient = asyncRequestClient;
            Method = method.ToString();
            Url = url;
            Metadata.Priority = priority;
            FulcrumAssert.IsNotNull(Metadata);
        }

        /// <inheritdoc />
        public AsyncHttpRequest AddHeader(string name, string[] values)
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(name);
            FulcrumAssert.IsNotNull(values);
            Headers ??= new Dictionary<string, StringValues>();
            if (Headers.TryGetValue(name, out var currentValues))
            {
                var newValues = currentValues.ToList();
                newValues.AddRange(values);
                Headers[name] = newValues.ToArray();
            }
            else
            {
                Headers.Add(name, new StringValues(values));
            }
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest AddHeader(string name, string value)
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(name);
            var values = new string[] { value };

            return AddHeader(name, values);
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetContent(string content, string contentType)
        {
            if (contentType != "application/json")
            {
                throw new FulcrumNotImplementedException("The only supported content type currently is application/json.");
            }
            Content = content;
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetContent(JToken content)
        {
            return SetContent(content?.ToString(Formatting.Indented), "application/json");
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetContentAsJson(object content)
        {
            return SetContent(content == null ? null : JsonConvert.SerializeObject(content, Formatting.Indented), "application/json");
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetExecuteBefore(DateTimeOffset before)
        {
            InternalContract.RequireGreaterThanOrEqualTo(DateTimeOffset.UtcNow, before, nameof(before));
            Metadata.ExecuteBefore = before;
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetExecuteBefore(TimeSpan before)
        {
            InternalContract.RequireGreaterThanOrEqualTo(TimeSpan.Zero, before, nameof(before));
            Metadata.ExecuteBefore = DateTimeOffset.UtcNow + before;
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetExecuteAfter(DateTimeOffset after)
        {
            Metadata.ExecuteAfter = after;
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetExecuteAfter(TimeSpan after)
        {
            InternalContract.RequireGreaterThanOrEqualTo(TimeSpan.Zero, after, nameof(after));
            Metadata.ExecuteBefore = DateTimeOffset.UtcNow + after;
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetCallbackUrl(string url)
        {
            InternalContract.RequireNotNullOrWhiteSpace(url, nameof(url));
            Metadata.Callback ??= new RequestCallback();
            Metadata.Callback.Url = url;
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetCallbackUrl(Uri uri)
        {
            InternalContract.RequireNotNull(uri, nameof(uri));
            return SetCallbackUrl(uri.AbsoluteUri);
        }

        /// <inheritdoc />
        public AsyncHttpRequest AddCallbackHeader(string name, string[] values)
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(name);
            FulcrumAssert.IsNotNull(values);
            Metadata.Callback ??= new RequestCallback();
            Metadata.Callback.Headers ??= new Dictionary<string, StringValues>();
            if (Metadata.Callback.Headers.TryGetValue(name, out var currentValues))
            {
                var newValues = currentValues.ToList();
                newValues.AddRange(values);
                Metadata.Callback.Headers[name] = newValues.ToArray();
            }
            else
            {
                Metadata.Callback.Headers.Add(name, new StringValues(values));
            }
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest AddCallbackHeader(string name, string value)
        {
            FulcrumAssert.IsNotNull(value);
            var values = new string[] { value };

            return AddCallbackHeader(name, values);
        }

        /// <inheritdoc />
        public async Task<string> SendAsync(CancellationToken cancellationToken = default)
        {
            FulcrumAssert.IsValidated(this, CodeLocation.AsString());
            return await _asyncRequestClient.SendRequestAsync(this, cancellationToken);
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetCallbackContext(string context)
        {
            Metadata.Callback ??= new RequestCallback();
            Metadata.Callback.Context = context;
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest SetCallbackContextAsJson(object context)
        {
            Metadata.Callback ??= new RequestCallback();
            SetCallbackContext(context == null ? null : JsonConvert.SerializeObject(context));
            return this;
        }

        /// <inheritdoc />
        public AsyncHttpRequest AddHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (var requestHeader in headers)
            {
                AddHeader(requestHeader.Key, requestHeader.Value.ToArray());
            }

            return this;
        }
    }
}
