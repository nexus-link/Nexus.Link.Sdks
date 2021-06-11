using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.AsyncCaller.Sdk.Common.Models
{
    /// <summary>
    /// https://github.com/aliostad/CacheCow.git
    /// Does not close the stream since the stream can be used to store other objects
    /// so it has to be closed in the client
    /// </summary>
    public class MessageContentHttpMessageSerializer : IHttpMessageSerializerAsync
    {
        private readonly bool _bufferContent;

        public MessageContentHttpMessageSerializer()
            : this(false)
        {
        }

        public MessageContentHttpMessageSerializer(bool bufferContent)
        {
            _bufferContent = bufferContent;
        }

        public async Task SerializeAsync(HttpRequestMessage request, Stream stream, CancellationToken cancellationToken = default)
        {
            if (request == null) return;
            if (_bufferContent && request.Content != null)
            {
                try
                {
                    await request.Content.LoadIntoBufferAsync();
                }
                catch (Exception e)
                {
                    // Note! Do use AsyncCallerLogger here, since this is used by the SDK.
                    Log.LogError("Unexpected exception A283A24C-EC3E-4E8A-81A3-3B90701B24AC", e);
                }
            }
            var httpMessageContent = new HttpMessageContent(request);
            await SerializeAsync(httpMessageContent, stream, cancellationToken);
        }

        public async Task<byte[]> SerializeAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request == null) return null;
            using (var memoryStream = new MemoryStream())
            {
                await SerializeAsync(request, memoryStream, cancellationToken);
                return memoryStream.ToArray();
            }
        }

        public async Task SerializeAsync(HttpResponseMessage response, Stream stream, CancellationToken cancellationToken = default)
        {
            if (response == null) return;
            if (_bufferContent && response.Content != null)
            {
                await response.Content.LoadIntoBufferAsync();
            }

            var httpMessageContent = new HttpMessageContent(response);
            await SerializeAsync(httpMessageContent, stream, cancellationToken);
        }

        public async Task<byte[]> SerializeAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (response == null) return null;
            using (var memoryStream = new MemoryStream())
            {
                await SerializeAsync(response, memoryStream, cancellationToken);
                return memoryStream.ToArray();
            }
        }

        private static async Task SerializeAsync(HttpContent httpMessageContent, Stream stream, CancellationToken cancellationToken = default)
        {
            var buffer = await httpMessageContent.ReadAsByteArrayAsync();
            await Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite,
                buffer, 0, buffer.Length, null, TaskCreationOptions.AttachedToParent);
        }

        public async Task<HttpRequestMessage> DeserializeToRequestAsync(Stream stream, string uriScheme, CancellationToken cancellationToken = default)
        {
            return await DeserializeToRequestAsync(new StreamContent(stream), uriScheme, cancellationToken);
        }

        public async Task<HttpRequestMessage> DeserializeToRequestAsync(byte[] byteArray, string uriScheme, CancellationToken cancellationToken = default)
        {
            if (byteArray == null) return null;
            return await DeserializeToRequestAsync(new ByteArrayContent(byteArray), uriScheme, cancellationToken);
        }

        private static async Task<HttpRequestMessage> DeserializeToRequestAsync(HttpContent content, string uriScheme, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage { Content = content };
            request.Content.Headers.Add("Content-Type", "application/http;msgtype=request");

            try
            {
                if (string.IsNullOrWhiteSpace(uriScheme)) return await request.Content.ReadAsHttpRequestMessageAsync(cancellationToken);
                return await request.Content.ReadAsHttpRequestMessageAsync(uriScheme, cancellationToken);

            }
            catch (Exception)
            {
                var httpResponseMessage = await MaybeRemoveExpiresHeader(content, "application/http;msgtype=request", cancellationToken);
                if (httpResponseMessage != null)
                {
                    if (string.IsNullOrWhiteSpace(uriScheme)) return await request.Content.ReadAsHttpRequestMessageAsync(cancellationToken);
                    return await request.Content.ReadAsHttpRequestMessageAsync(uriScheme, cancellationToken);
                }
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeserializeToResponseAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            return await DeserializeToResponseAsync(new StreamContent(stream), cancellationToken);
        }

        public async Task<HttpResponseMessage> DeserializeToResponseAsync(byte[] byteArray, CancellationToken cancellationToken = default)
        {
            if (byteArray == null) return null;
            return await DeserializeToResponseAsync(new ByteArrayContent(byteArray), cancellationToken);
        }

        private static async Task<HttpResponseMessage> DeserializeToResponseAsync(HttpContent content, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage { Content = content };
            response.Content.Headers.Add("Content-Type", "application/http;msgtype=response");

            try
            {
                return await response.Content.ReadAsHttpResponseMessageAsync(cancellationToken);
            }
            catch (Exception)
            {
                var httpResponseMessage = await MaybeRemoveExpiresHeader(content, "application/http;msgtype=response", cancellationToken);
                if (httpResponseMessage != null)
                {
                    response = await httpResponseMessage.Content.ReadAsHttpResponseMessageAsync(cancellationToken);
                    return response;
                }
                throw;
            }
        }

        private static readonly Regex ServerHeaderRegex = new Regex("^Server: (.*,.*)$", RegexOptions.Multiline);

        private static async Task<HttpResponseMessage> MaybeRemoveExpiresHeader(HttpContent originalContent, string msgTypeHeader, CancellationToken cancellationToken)
        {
            try
            {
                // Bug in dot net core 1: Expires header = -1
                // Bug in dot net core 2: Can't handle comma (,) in Server header
                await originalContent.LoadIntoBufferAsync();
                var originalContentAsString = await originalContent.ReadAsStringAsync();

                // Do not replace in body part of content
                var contentIndex = originalContentAsString.IndexOf("\r\n\r\n", StringComparison.Ordinal); // https://www.w3.org/Protocols/rfc2616/rfc2616-sec6.html
                string headers;
                var body = "";
                if (contentIndex != -1)
                {
                    // Keep one CRLF in the header part (to correctly replace 'Expires: -1' if it is last)
                    contentIndex += 2;

                    headers = originalContentAsString.Substring(0, contentIndex);
                    body = originalContentAsString.Substring(contentIndex);
                }
                else
                {
                    headers = originalContentAsString;
                }

                if (headers.Contains("Expires: -1") || ServerHeaderRegex.IsMatch(headers))
                {
                    headers = headers.Replace("Expires: -1\r\n", "");
                    headers = ServerHeaderRegex.Replace(headers, match => "Server: " + match.Groups[1].Value.Replace(",", ""));
                    var contentAsString = headers + body;
                    var response = new HttpResponseMessage { Content = new StringContent(contentAsString) };

                    // Prepare for the ReadAsHttpResponseMessageAsync() method by setting the Content-Type header to "application/http; msgtype=..."
                    response.Content.Headers.Remove("Content-Type");
                    response.Content.Headers.Add("Content-Type", msgTypeHeader);

                    if (originalContentAsString.Contains("Expires: -1")) Log.LogVerbose("Removed header 'Expires: -1' to compensate for dot net core bug");
                    if (ServerHeaderRegex.IsMatch(originalContentAsString)) Log.LogVerbose("Removed comma from Server header to compensate for dot net core bug");
                    return response;
                }

            }
            catch (Exception e)
            {
                Log.LogError("Error trying to compensate for dotnet core bugs (replace header Expires: -1 or comma in Server header)", e);
            }
            return null;
        }
    }
}
