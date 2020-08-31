using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.AsyncCaller.Sdk.Helpers
{
    /// <summary>
    /// https://github.com/aliostad/CacheCow.git
    /// Does not close the stream since the stream can be used to store other objects
    /// so it has to be closed in the client
    /// </summary>
    [Obsolete("Please use Nexus.Link.AsyncCaller.Common.Models.MessageContentHttpMessageSerializer")]
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
            var memoryStream = new MemoryStream();
            await SerializeAsync(request, memoryStream);
            return memoryStream.ToArray();
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
            var memoryStream = new MemoryStream();
            await SerializeAsync(response, memoryStream);
            return memoryStream.ToArray();
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
            if (string.IsNullOrWhiteSpace(uriScheme)) return await request.Content.ReadAsHttpRequestMessageAsync();
            return await request.Content.ReadAsHttpRequestMessageAsync(uriScheme);
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
            return await response.Content.ReadAsHttpResponseMessageAsync();
        }
    }
}
