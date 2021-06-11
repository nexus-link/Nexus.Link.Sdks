using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.AsyncCaller.Sdk.Common.Models;
using Nexus.Link.AsyncCaller.Sdk.Data.Models;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Threads;
using Convert = Nexus.Link.AsyncCaller.Sdk.Common.Helpers.Convert;

namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models
{
    /// <summary>
    /// Represents a request for us to try to call the specified url and come back with the result
    /// </summary>
    public class Request
    {
        public string Id { get; set; }
        public JToken Context { get; set; }
        public int? Priority { get; set; }
        public HttpRequestMessage CallOut { get; set; }
        public HttpRequestMessage CallBack { get; set; }

        public override string ToString()
        {
            var request = $"{CallOut.Method} {CallOut.RequestUri} ({Id})";
            if (CallOut.Content == null) return request;
            if (FulcrumApplication.IsInProduction || FulcrumApplication.IsInProductionSimulation) return request;
            try
            {
                CallOut.Content.LoadIntoBufferAsync().Wait();
                var content = ThreadHelper.CallAsyncFromSync(async () => await CallOut.Content.ReadAsStringAsync());
                return $"{request} {content}";
            }
            catch
            {
                // CallOut.Content could have been read
                return request;
            }
        }

        public static async Task<Request> FromRawAsync(RawRequest source, CancellationToken cancellationToken = default)
        {
            if (source == null) return null;
            var serializer = new MessageContentHttpMessageSerializer(true);
            var target = new Request
            {
                Id = source.Id,
                Context = Convert.ToJson(source.Context),
                Priority = source.Priority,
                CallOut = await serializer.DeserializeToRequestAsync(source.CallOut, source.CallOutUriScheme, cancellationToken),
                CallBack = await serializer.DeserializeToRequestAsync(source.CallBack, source.CallBackUriScheme, cancellationToken)
            };
            return target;
        }

        public async Task<RawRequest> ToRawAsync(CancellationToken cancellationToken = default)
        {
            var serializer = new MessageContentHttpMessageSerializer(true);
            var target = new RawRequest
            {
                Id = Id,
                Context = Convert.ToByteArray(Context),
                Priority = Priority,
                CallOut = await serializer.SerializeAsync(CallOut, cancellationToken),
                CallOutUriScheme = CallOut.RequestUri.Scheme,
                CallBack = await serializer.SerializeAsync(CallBack, cancellationToken),
                CallBackUriScheme = CallBack?.RequestUri?.Scheme
            };
            target.SetTitle(CallOut.Method.ToString(), CallOut.RequestUri);
            return target;
        }
    }
}

