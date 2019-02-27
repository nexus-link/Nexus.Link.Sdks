using Newtonsoft.Json;

namespace Nexus.Link.KeyTranslator.Sdk.Models
{
    public class InstancesExistsResult
    {
        [JsonProperty(Order = 0)]
        public bool Exists { get; set; }

        [JsonProperty(Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}
