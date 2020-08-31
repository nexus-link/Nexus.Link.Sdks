using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nexus.Link.AsyncCaller.Sdk.Helpers
{
    /// <summary>
    /// https://github.com/aliostad/CacheCow.git
    /// </summary>
    [Obsolete("Please use Nexus.Link.AsyncCaller.Common.Models.IHttpMessageSerializerAsync")]
    public interface IHttpMessageSerializerAsync
    {
        Task SerializeAsync(HttpResponseMessage response, Stream stream);
        Task SerializeAsync(HttpRequestMessage request, Stream stream);
        Task<HttpResponseMessage> DeserializeToResponseAsync(Stream stream);
        Task<HttpRequestMessage> DeserializeToRequestAsync(Stream stream, string uriScheme);
    }
}
