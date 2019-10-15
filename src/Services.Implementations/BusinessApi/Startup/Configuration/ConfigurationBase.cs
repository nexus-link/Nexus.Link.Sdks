using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

#pragma warning disable 1591

namespace Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration
{
    public abstract class ConfigurationBase : IConfiguration
    {
        public IConfiguration Configuration { get; }

        public string Path { get; }

        protected ConfigurationBase(ConfigurationBase parent, string sectionName)
        {
            Configuration = parent.GetMandatorySection(sectionName);
            Path = parent.NewPath(sectionName);
        }

        protected ConfigurationBase(IConfiguration configuration)
        {
            Configuration = configuration;
            Path = "";
        }

        protected T GetMandatoryValue<T>(string key)
        {
            var value = Configuration.GetValue<T>(key);
            FulcrumAssert.IsNotNull(value, CodeLocation.AsString(), $"You must provide a configuration value for {NewPath(key)}");
            FulcrumAssert.IsNotDefaultValue(value, CodeLocation.AsString(), $"You must provide a configuration value for {NewPath(key)} other than the default value for the type ({default(T)})");
            if (typeof(string).IsAssignableFrom(typeof(T))) FulcrumAssert.IsNotNullOrWhiteSpace((string)(object)value, CodeLocation.AsString(), $"You must provide a value for {NewPath(key)} that is not the empty string and not only white space.");
            return value;
        }

        protected IConfigurationSection GetMandatorySection(string key)
        {
            var value = GetSection(key);
            FulcrumAssert.IsNotNull(value, CodeLocation.AsString(), $"You must provide the configuration section {NewPath(key)}");
            return value;
        }

        /// <inheritdoc />
        public IConfigurationSection GetSection(string key)
        {
            return Configuration.GetSection(key);
        }

        /// <inheritdoc />
        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return Configuration.GetChildren();
        }

        /// <inheritdoc />
        public IChangeToken GetReloadToken()
        {
            return Configuration.GetReloadToken();
        }

        /// <inheritdoc />
        public string this[string key]
        {
            get => Configuration[key];
            set => Configuration[key] = value;
        }

        protected string NewPath(string key)
        {
            return string.IsNullOrWhiteSpace(Path) ? key : $"{Path}:{key}";
        }
    }
}