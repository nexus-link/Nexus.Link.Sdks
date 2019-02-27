using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#pragma warning disable 1591

namespace Nexus.Link.BusinessEvents.Sdk.RestClients.Models
{
    public class PublicationTestResult
    {
        [JsonProperty(Order = 0)]
        public bool Verified { get; set; }

        [JsonProperty(Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Errors { get; set; }

        [JsonProperty(Order = 2)]
        public JObject Contract { get; set; }

        [JsonProperty(Order = 3)]
        public JObject Payload { get; set; }
    }
}