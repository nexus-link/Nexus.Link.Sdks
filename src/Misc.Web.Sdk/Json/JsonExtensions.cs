using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Json;

namespace Nexus.Link.Misc.Web.Sdk.Json
{
    /// <summary>
    /// Extensions for JSON
    /// </summary>
    public static class JsonExtensions
    {

        /// <summary>
        /// Set the serializer settings the way we expect them to behave in Nexus Link.
        /// Typically chain this to <see cref="Libraries.Core.Json.JsonExtensions.SetAsNexusLink"/>
        /// </summary>
        /// <param name="settings"></param>
        public static JsonSerializerSettings AddNexusWeb(this JsonSerializerSettings settings)
        {
            // Add a TimeSpan converter
            settings.Converters.Add(new Microsoft.Rest.Serialization.Iso8601TimeSpanConverter());

            settings.ContractResolver = new Microsoft.Rest.Serialization.ReadOnlyJsonContractResolver();
            return settings;
        }
    }
}
