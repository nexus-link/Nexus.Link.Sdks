using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.AsyncCaller.Common.Models;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Threads;
using Xlent.Lever.AsyncCaller.Data.Models;

namespace Nexus.Link.AsyncCaller.Dispatcher.Models
{
    /// <summary>
    /// Represents a request for us to try to call the specified url and come back with the result
    /// </summary>
    public class Request
    {
        public string Id { get; set; }
        public JToken Context { get; set; }
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

        public static async Task<Request> FromDataAsync(RawRequest source)
        {
            if (source == null) return null;
            var serializer = new MessageContentHttpMessageSerializer(true);
            var target = new Request
            {
                Id = source.Id,
                Context = Nexus.Link.AsyncCaller.Common.Helpers.Convert.ToJson(source.Context),
                CallOut = await serializer.DeserializeToRequestAsync(source.CallOut, source.CallOutUriScheme),
                CallBack = await serializer.DeserializeToRequestAsync(source.CallBack, source.CallBackUriScheme)
            };
            return target;
        }

        public async Task<RawRequest> ToDataAsync()
        {
            var serializer = new MessageContentHttpMessageSerializer(true);
            var target = new RawRequest
            {
                Id = Id,
                Context = Nexus.Link.AsyncCaller.Common.Helpers.Convert.ToByteArray(Context),
                CallOut = await serializer.SerializeAsync(CallOut),
                CallOutUriScheme = CallOut.RequestUri.Scheme,
                CallBack = await serializer.SerializeAsync(CallBack),
                CallBackUriScheme = CallBack?.RequestUri?.Scheme
            };
            target.SetTitle(CallOut.Method.ToString(), CallOut.RequestUri);
            return target;
        }
    }
}

