using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nexus.Link.AsyncCaller.Sdk.Common.Models
{
    public class ResponseContent
    {
        public string Id { get; set; }
        public JToken Context { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Payload { get; set; }
        public string PayloadMediaType { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ResponseContent Deserialize(string serializedString)
        {
            return JsonConvert.DeserializeObject<ResponseContent>(serializedString);
        }
    }
}