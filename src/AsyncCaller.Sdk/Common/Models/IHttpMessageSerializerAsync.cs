using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.AsyncCaller.Sdk.Common.Models
{
    /// <summary>
    /// https://github.com/aliostad/CacheCow.git
    /// </summary>
    public interface IHttpMessageSerializerAsync
    {
        Task SerializeAsync(HttpResponseMessage response, Stream stream, CancellationToken cancellationToken = default);
        Task SerializeAsync(HttpRequestMessage request, Stream stream, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeserializeToResponseAsync(Stream stream, CancellationToken cancellationToken = default);
        Task<HttpRequestMessage> DeserializeToRequestAsync(Stream stream, string uriScheme, CancellationToken cancellationToken = default);
    }
}
