using Microsoft.Extensions.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration
{
    /// <summary>
    /// Extensions for <see cref="IConfiguration"/>
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Same as GetValue{T}(), but does not accept the default value for T
        /// </summary>
        public static T GetMandatoryValue<T>(this IConfiguration configuration, string key)
        {
            var value = configuration.GetValue<T>(key);
            FulcrumAssert.IsNotNull(value, CodeLocation.AsString(), $"Missing configuration {key}");
            FulcrumAssert.IsNotDefaultValue(value, CodeLocation.AsString(), $"Configuration for {key} is either missing or set to {default(T)}, which is not accepted.");
            return value;
        }

        /// <summary>
        /// Same as GetValue{T}(), but does not accept the default value for T
        /// </summary>
        public static string GetMandatoryString(this IConfiguration configuration, string key)
        {
            var value = configuration.GetMandatoryValue<string>(key);
            FulcrumAssert.IsNotNullOrWhiteSpace(value, CodeLocation.AsString(), $"The configuration {key} must not be the empty string or only white space.");
            return value;
        }
    }
}
