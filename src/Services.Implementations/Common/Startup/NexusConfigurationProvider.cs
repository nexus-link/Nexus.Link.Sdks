using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

// https://medium.com/@dneimke/custom-configuration-in-net-core-2-193ff6f02046
namespace Nexus.Link.Services.Implementations.Startup
{
    /// <summary>
    /// A configuration provider that gets configuration data from the Nexus configuration database
    /// </summary>
    internal class NexusConfigurationProvider : ConfigurationProvider
    {
        private readonly string _authenticationClientId;
        private readonly IConfigurationService _configurationService;

        public NexusConfigurationProvider(string authenticationClientId, IConfigurationService configurationService)
        {
            _authenticationClientId = authenticationClientId;
            _configurationService = configurationService;
        }

        /// <inheritdoc />
        public override void Load()
        {
            try
            {
                var jToken = ThreadHelper.CallAsyncFromSync(async () =>
                    await _configurationService.ReadAsync(_authenticationClientId));

                LoadObject("", jToken);
            }
            catch (ConfigurationParseException e)
            {
                Log.LogWarning($"Could not read the configuration for application with authentication client id {_authenticationClientId}: {e.Message}");
                return;
            }
        }

        private void LoadObject(string prefix, JToken jToken)
        {
            if (!(jToken is JObject jObject))
            {
                throw new ConfigurationParseException(
                    $"Expected the configuration to be of type JSON object, but was {jToken.Type}");
            }
            foreach (var property in jObject.Properties())
            {
                LoadProperty($"{prefix}{property.Name}", property.Value);
            }
        }

        private void LoadProperty(string prefix, JToken propertyValue)
        {
            Data.Add(prefix, propertyValue?.ToString());
            if (propertyValue == null) return;
            switch (propertyValue.Type)
            {
                case JTokenType.Object:
                    LoadObject(prefix + ":", propertyValue);
                    break;
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Date:
                case JTokenType.Null:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                    break;
                //case JTokenType.None:
                //case JTokenType.Array:
                //case JTokenType.Constructor:
                //case JTokenType.Property:
                //case JTokenType.Comment:
                //case JTokenType.Undefined:
                //case JTokenType.Raw:
                default:
                    throw new ConfigurationParseException($"Did not expect the JSON type {propertyValue.Type} as the property value of {prefix}.");
            }
        }

        private class ConfigurationParseException : Exception
        {
            public ConfigurationParseException(string message) : base(message){}
        }
    }
}
