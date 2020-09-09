using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nexus.Link.AsyncCaller.Sdk.Common.Models
{
    /// <summary>
    /// https://github.com/aliostad/CacheCow.git
    /// </summary>
    public interface IHttpMessageSerializerAsync
    {
        Task SerializeAsync(HttpResponseMessage response, Stream stream);
        Task SerializeAsync(HttpRequestMessage request, Stream stream);
        Task<HttpResponseMessage> DeserializeToResponseAsync(Stream stream);
        Task<HttpRequestMessage> DeserializeToRequestAsync(Stream stream, string uriScheme);
    }
}
