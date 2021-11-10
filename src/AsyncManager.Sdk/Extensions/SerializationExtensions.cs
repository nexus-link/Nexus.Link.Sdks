using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Microsoft.Rest;
using Nexus.Link.AsyncManager.Sdk.RestClients;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk.Extensions
{
    /// <summary>
    /// Extension methods for serializing HttpResponse and HttpRequest
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Deserialize to a <see cref="HttpResponseMessage"/>.
        /// </summary>
        public static HttpResponseMessage ToHttpResponseMessage(this HttpResponse source, HttpRequestMessage request)
        {
            InternalContract.Require(source.HttpStatus != null, $"{nameof(HttpResponse.HttpStatus)} must not be null.");
            // ReSharper disable once PossibleInvalidOperationException
            var target = request.CreateResponse(source.HttpStatus.Value.ToEnum<HttpStatusCode>());
            if (source.Headers != null)
            {
                foreach (var header in source.Headers)
                {
                    if (header.Key.ToLowerInvariant().StartsWith("content-")) continue;
                    target.Headers.Add(header.Key, header.Value.ToArray());
                }
            }

            target.Content = source.Content == null ? null : new StringContent(source.Content, System.Text.Encoding.UTF8, "application/json");
            return target;
        }

        /// <summary>
        /// Serialize to <see cref="HttpResponseMessage"/>.
        /// </summary>
        public static async Task<HttpRequestCreate> FromAsync(this HttpRequestCreate target, HttpRequestMessage source, double priority, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            target.Method = source.Method.ToString();
            target.Url = source.RequestUri.AbsoluteUri;
            target.Metadata.Priority = priority;

            // Headers
            target.Headers ??= new Dictionary<string, StringValues>();
            target.Headers.From(source.Headers);
            string contentAsString = null;
            if (source.Content != null)
            {
                await source.Content.LoadIntoBufferAsync();
                contentAsString = await source.Content.ReadAsStringAsync();
                var contentHeaders = source.GetContentHeaders();
                if (contentHeaders != null && contentHeaders.TryGetValues("Content-Type", out var values))
                {
                    var array = values.ToArray();
                    InternalContract.Require(array.Length == 1, $"Expected exactly one value for header Content-Type, was {string.Join("|", array)}");
                    InternalContract.Require(array[0].Contains("application/json"),
                        "The only supported content type currently is application/json.");
                    InternalContract.Require(array[0].Contains("utf-8"),
                        "The only supported content encoding currently is utf-8.");
                }
            }

            target.Content = contentAsString;
            return target;
        }

        private static Dictionary<string, StringValues> From(this Dictionary<string, StringValues> target,
            HttpRequestHeaders source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));

            foreach (var requestHeader in source)
            {
                target.Add(requestHeader.Key, requestHeader.Value.ToArray());
            }

            return target;
        }
    }
}
