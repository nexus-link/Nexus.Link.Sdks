using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
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

        public async Task SerializeAsync(HttpRequestMessage request, Stream stream)
        {
            if (request == null) return;
            if (_bufferContent && (request.Content != null))
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
            await SerializeAsync(httpMessageContent, stream);
        }

        public async Task<byte[]> SerializeAsync(HttpRequestMessage request)
        {
            if (request == null) return null;
            using (var memoryStream = new MemoryStream())
            {
                await SerializeAsync(request, memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task SerializeAsync(HttpResponseMessage response, Stream stream)
        {
            if (response == null) return;
            if (_bufferContent && (response.Content != null))
            {
                await response.Content.LoadIntoBufferAsync();
            }

            var httpMessageContent = new HttpMessageContent(response);
            await SerializeAsync(httpMessageContent, stream);
        }

        public async Task<byte[]> SerializeAsync(HttpResponseMessage response)
        {
            if (response == null) return null;
            using (var memoryStream = new MemoryStream())
            {
                await SerializeAsync(response, memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static async Task SerializeAsync(HttpContent httpMessageContent, Stream stream)
        {
            var buffer = await httpMessageContent.ReadAsByteArrayAsync();
            await Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite,
                buffer, 0, buffer.Length, null, TaskCreationOptions.AttachedToParent);
        }

        public async Task<HttpRequestMessage> DeserializeToRequestAsync(Stream stream, string uriScheme)
        {
            return await DeserializeToRequestAsync(new StreamContent(stream), uriScheme);
        }

        public async Task<HttpRequestMessage> DeserializeToRequestAsync(byte[] byteArray, string uriScheme)
        {
            if (byteArray == null) return null;
            return await DeserializeToRequestAsync(new ByteArrayContent(byteArray), uriScheme);
        }

        private static async Task<HttpRequestMessage> DeserializeToRequestAsync(HttpContent content, string uriScheme)
        {
            var request = new HttpRequestMessage { Content = content };
            request.Content.Headers.Add("Content-Type", "application/http;msgtype=request");

            try
            {
                if (string.IsNullOrWhiteSpace(uriScheme)) return await request.Content.ReadAsHttpRequestMessageAsync();
                return await request.Content.ReadAsHttpRequestMessageAsync(uriScheme);

            }
            catch (Exception)
            {
                var httpResponseMessage = await MaybeRemoveExpiresHeader(content, "application/http;msgtype=request");
                if (httpResponseMessage != null)
                {
                    if (string.IsNullOrWhiteSpace(uriScheme)) return await request.Content.ReadAsHttpRequestMessageAsync();
                    return await request.Content.ReadAsHttpRequestMessageAsync(uriScheme);
                }
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeserializeToResponseAsync(Stream stream)
        {
            return await DeserializeToResponseAsync(new StreamContent(stream));
        }

        public async Task<HttpResponseMessage> DeserializeToResponseAsync(byte[] byteArray)
        {
            if (byteArray == null) return null;
            return await DeserializeToResponseAsync(new ByteArrayContent(byteArray));
        }

        private static async Task<HttpResponseMessage> DeserializeToResponseAsync(HttpContent content)
        {
            var response = new HttpResponseMessage { Content = content };
            response.Content.Headers.Add("Content-Type", "application/http;msgtype=response");

            try
            {
                return await response.Content.ReadAsHttpResponseMessageAsync();
            }
            catch (Exception)
            {
                var httpResponseMessage = await MaybeRemoveExpiresHeader(content, "application/http;msgtype=response");
                if (httpResponseMessage != null)
                {
                    response = await httpResponseMessage.Content.ReadAsHttpResponseMessageAsync();
                    return response;
                }
                throw;
            }
        }

        private static readonly Regex ServerHeaderRegex = new Regex("^Server: (.*,.*)$", RegexOptions.Multiline);

        private static async Task<HttpResponseMessage> MaybeRemoveExpiresHeader(HttpContent originalContent, string msgTypeHeader)
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
