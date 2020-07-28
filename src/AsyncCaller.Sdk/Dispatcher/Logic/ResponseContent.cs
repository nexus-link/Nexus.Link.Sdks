using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CM = Nexus.Link.AsyncCaller.Common.Models;

namespace Nexus.Link.AsyncCaller.Dispatcher.Logic
{
    public class ResponseContent
    {
        public string Id { get; set; }
        public JToken Context { get; set; }
        public HttpContent Payload { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public static ResponseContent FromData(CM.ResponseContent source)
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

        public async Task<CM.ResponseContent> ToDataAsync()
        {
            await Payload.LoadIntoBufferAsync();
            var target = new CM.ResponseContent
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