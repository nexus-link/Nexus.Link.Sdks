using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.AsyncCaller.Sdk.Common.Models;
using Nexus.Link.AsyncCaller.Sdk.Data.Models;
using Nexus.Link.Libraries.Core.Assert;

#pragma warning disable 1591

namespace Nexus.Link.AsyncCaller.Sdk
{
    /// <summary>
    /// The information for a specific asynchronous call
    /// </summary>
    public class AsyncCall : IAsyncCall
    {
        private readonly IAsyncCaller _asyncCaller;

        public AsyncCall(IAsyncCaller asyncCaller, HttpMethod method, Uri uri)
        {
            InternalContract.RequireNotNull(asyncCaller, nameof(asyncCaller));
            InternalContract.RequireNotNull(method, nameof(method));
            InternalContract.RequireNotNull(uri, nameof(uri));


            _asyncCaller = asyncCaller;
            Id = Guid.NewGuid().ToString();
            CallOut = new HttpRequestMessage(method, uri);
        }

        internal AsyncCall(AsyncCaller asyncCaller, HttpMethod method, string uri)
            : this(asyncCaller, method, new Uri(uri))
        {
            var isValidUrl = Uri.IsWellFormedUriString(uri, UriKind.Absolute);
            InternalContract.Require(isValidUrl, $"The submitted URL ({uri}) is not well formed.");
        }

        public string Id { get; set; }
        public HttpRequestMessage CallOut { get; internal set; }
        public HttpRequestMessage CallBack { get; internal set; }
        public JToken Context { get; set; }
        public int? Priority { get; set; }

        public IAsyncCall SetJsonContent(JToken content)
        {
            var c = content == null ? null : new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            return SetContent(c);
        }

        public IAsyncCall SetTextContent(string content)
        {
            var c = content == null ? null : new StringContent(content, Encoding.UTF8, "text/plain");
            return SetContent(c);
        }

        public IAsyncCall SetXmlContent(string content)
        {
            var c = content == null ? null : new StringContent(content, Encoding.UTF8, "application/xml");
            return SetContent(c);
        }

        public IAsyncCall SetContent(HttpContent content)
        {
            CallOut.Content = content;
            return this;
        }

        public IAsyncCall SetContext(JToken context)
        {
            Context = context;
            return this;
        }

        public IAsyncCall SetPriority(int? priority)
        {
            Priority = priority;
            return this;
        }

        public IAsyncCall SetId(string id)
        {
            Id = id;
            return this;
        }

        public IAsyncCall SetId(Guid id)
        {
            return SetId(id.ToString());
        }

        public IAsyncCall SetCallback(string uri)
        {
            return SetCallback(new Uri(uri));
        }

        public IAsyncCall SetCallback(Uri uri)
        {
            return SetCallback(HttpMethod.Post, uri);
        }

        public IAsyncCall SetCallback(HttpMethod method, Uri uri)
        {
            CallBack = new HttpRequestMessage(method, uri);
            return this;
        }

        public IAsyncCall SetCallback(HttpMethod method, string uri)
        {
            return SetCallback(method, new Uri(uri));
        }
        public IAsyncCall AddOutHeader(string name, string value)
        {
            CallOut.Headers.Add(name, value);
            return this;
        }

        public IAsyncCall AddCallbackHeader(string name, string value)
        {
            InternalContract.Require(CallBack != null, $"A callback has to be set before adding headers to it ({this}).");
            CallBack?.Headers.Add(name, value);
            return this;
        }

        public IAsyncCall AddOutAuthorization(string value)
        {
            return AddOutHeader("Authorization", value);
        }

        public IAsyncCall AddCallbackAuthorization(string value)
        {
            return AddCallbackHeader("Authorization", value);
        }

        public IAsyncCall AddOutBearerAuthorization(string token)
        {
            return AddOutAuthorization($"Bearer {token}");
        }

        public IAsyncCall AddCallbackBearerAuthorization(string token)
        {
            return AddCallbackAuthorization($"Bearer {token}");
        }

        public virtual async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var rawRequest = await GetRawRequestAsync(cancellationToken);
            return await _asyncCaller.ExecuteAsync(rawRequest);
        }

        public override string ToString()
        {
            return $"{CallOut.Method} {CallOut.RequestUri} ({Id})";
        }

        private async Task<RawRequest> GetRawRequestAsync(CancellationToken cancellationToken)
        {
            var serializer = new MessageContentHttpMessageSerializer(true);
            var callOutTask = serializer.SerializeAsync(CallOut);
            var callBackTask = serializer.SerializeAsync(CallBack);
            var callOut = await callOutTask;
            var callBack = await callBackTask;

            return new RawRequest
            {
                Id = Id,
                Title = ToString(),
                CallOut = callOut,
                CallOutUriScheme = CallOut?.RequestUri?.Scheme,
                CallBack = callBack,
                CallBackUriScheme = CallBack?.RequestUri?.Scheme,
                Context = Context == null ? null : Encoding.UTF8.GetBytes(Context.ToString(Formatting.None)),
                Priority = Priority
            };
        }
    }
}
