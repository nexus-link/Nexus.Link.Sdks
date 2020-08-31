using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Common.Models;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.AsyncCaller.Sdk.RestClients.Facade.Models
{
    public class RequestMessage
    {
        public RequestMessage(string method, string requestUri)
        {
            Method = method;
            RequestUri = requestUri;
        }

        public string Method { get; set; }
        public string RequestUri { get; set; }
        public KeyValuePair<string, string[]>[] Headers { get; set; }
        public byte[] ContentAsByteArray { get; set; }
        public string ContentType { get; set; }

        public static async Task<RequestMessage> FromDataAsync(byte[] byteArray, string uriScheme)
        {
            if (byteArray == null) return null;
            var serializer = new MessageContentHttpMessageSerializer(true);
            var source = await serializer.DeserializeToRequestAsync(byteArray, uriScheme);
            var target = new RequestMessage(source.Method.Method, source.RequestUri.AbsoluteUri)
            {
                ContentAsByteArray = await source.Content.ReadAsByteArrayAsync(),
                ContentType = source.Content.Headers.ContentType.MediaType,
                Headers = FromData(source.Headers)
            };
            return target;
        }

        public async Task<byte[]> ToDataAsync()
        {
            var target = new HttpRequestMessage(MethodToData(Method), new Uri(RequestUri))
            {
                Content = ContentAsByteArray == null ? null : new ByteArrayContent(ContentAsByteArray)
            };
            if (target.Content != null && ContentType != null) target.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);
            ToData(Headers, target.Headers);
            var serializer = new MessageContentHttpMessageSerializer(true);
            return await serializer.SerializeAsync(target);
        }

        private HttpMethod MethodToData(string method)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            switch (method.ToUpper())
            {
                case "POST": return HttpMethod.Post;
                case "GET": return HttpMethod.Get;
                case "PUT": return HttpMethod.Put;
                case "DELETE": return HttpMethod.Delete;
                case "HEAD": return HttpMethod.Head;
                case "OPTIONS": return HttpMethod.Options;
                case "TRACE": return HttpMethod.Trace;
                default:
                    throw new FulcrumNotImplementedException($"Unknown HTTP method: {method}.");
            }
        }

        private static KeyValuePair<string, string[]>[] FromData(IEnumerable<KeyValuePair<string, IEnumerable<string>>> source)
        {
            return source.Select(keyValuePair => new KeyValuePair<string, string[]>(keyValuePair.Key, keyValuePair.Value.ToArray())).ToArray();
        }

        private static void ToData(IEnumerable<KeyValuePair<string, string[]>> source, HttpHeaders target)
        {
            if (source == null) return;
            foreach (var keyValuePair in source)
            {
                target.Add(keyValuePair.Key, keyValuePair.Value.ToArray());
            }
        }
    }
}
