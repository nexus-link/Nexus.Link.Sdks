using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Logic
{
    public class ResponseContent
    {
        public string Id { get; set; }
        public JToken Context { get; set; }
        public HttpContent Payload { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public static ResponseContent FromData(Common.Models.ResponseContent source)
        {
            var target = new ResponseContent
            {
                Id = source.Id,
                Context = source.Context,
                Payload = new StringContent(source.Payload, Encoding.UTF8, source.PayloadMediaType),
                StatusCode = source.StatusCode
            };
            return target;
        }

        public async Task<Common.Models.ResponseContent> ToDataAsync()
        {
            await Payload.LoadIntoBufferAsync();
            var target = new Common.Models.ResponseContent
            {
                Id = Id,
                Context = Context,
                Payload = await Payload.ReadAsStringAsync(),
                PayloadMediaType = Payload.Headers.ContentType?.MediaType ?? "application/json",
                StatusCode = StatusCode
            };
            return target;
        }
    }
}