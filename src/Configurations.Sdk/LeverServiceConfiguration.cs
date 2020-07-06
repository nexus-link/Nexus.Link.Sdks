using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.Platform.Authentication;

namespace Nexus.Link.Configurations.Sdk
{
    /// <summary>
    /// Very much the same as <see cref="ILeverConfigurationsManager"/>, but handles the authentication by itself.
    /// </summary>
    /// <remarks>To change the time the configuration is cached, use app setting LeverServiceConfiguration.Cache.TimeSpan</remarks>
    public class LeverServiceConfiguration : ILeverServiceConfiguration
    {
        private static readonly ConfigurationCache ConfigurationCache = new ConfigurationCache();
        private readonly LeverConfigurationsManager _configurationsManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceTenant">The tenant for the current service.</param>
        /// <param name="serviceName">The current service.</param>
        /// <param name="authenticationServiceUrl">A URL to the authentication service.</param>
        /// <param name="serviceCredentials">AuthenticationCredentials for the current service to the configuration service.</param>
        /// <param name="configurationsServiceUrl">A URL to the configuration service.</param>
        /// <param name="minimumTimeSpan">The minimum remaining time span for an existing token.</param>
        /// <param name="maximumTimeSpan">The maximum time span that we want for new tokens.</param>
        public LeverServiceConfiguration(Tenant serviceTenant, string serviceName, string authenticationServiceUrl, IAuthenticationCredentials serviceCredentials, string configurationsServiceUrl, TimeSpan minimumTimeSpan, TimeSpan maximumTimeSpan)
        {
            InternalContract.RequireNotNull(serviceTenant, nameof(serviceTenant));
            InternalContract.RequireNotNull(serviceName, nameof(serviceName));
            InternalContract.RequireNotNull(serviceCredentials, nameof(serviceCredentials));
            InternalContract.RequireNotNull(configurationsServiceUrl, nameof(configurationsServiceUrl));

            ServiceTenant = serviceTenant;
            ServiceName = serviceName;
            var authenticationManager = new NexusAuthenticationManager(ServiceTenant, authenticationServiceUrl);
            TokenRefresher = authenticationManager.CreateTokenRefresher(serviceCredentials, minimumTimeSpan, maximumTimeSpan);
            _configurationsManager = new LeverConfigurationsManager(configurationsServiceUrl, ServiceName, TokenRefresher.GetServiceClient());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceTenant">The tenant for the current service.</param>
        /// <param name="serviceName">The current service.</param>
        /// <param name="authenticationServiceUrl">A URL to the authentication service.</param>
        /// <param name="serviceCredentials">AuthenticationCredentials for the current service to the configuration service.</param>
        /// <param name="configurationsServiceUrl">A URL to the configuration service.</param>
        public LeverServiceConfiguration(Tenant serviceTenant, string serviceName, string authenticationServiceUrl, IAuthenticationCredentials serviceCredentials, string configurationsServiceUrl)
            : this(serviceTenant, serviceName, authenticationServiceUrl, serviceCredentials, configurationsServiceUrl, TimeSpan.FromHours(1), TimeSpan.FromHours(24))
        {
        }

        public ITokenRefresherWithServiceClient TokenRefresher { get; }

        /// <inheritdoc />
        public Tenant ServiceTenant { get; }

        /// <inheritdoc />
        public string ServiceName { get; }

        /// <inheritdoc />
        public async Task<ILeverConfiguration> GetConfigurationForAsync(Tenant tenant)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));

            var configuration = ConfigurationCache.Get(tenant);
            if (configuration != null) return configuration;

            if (tenant.Equals(FulcrumApplication.Setup.Tenant))
            {
                configuration = ReadConfigurationFromAppSettings();
            }
            else
            {
                configuration = await ReadConfigurationFromFundamentals(tenant);
            }

            ConfigurationCache.Add(tenant, configuration);
            return configuration;
        }

        private async Task<ILeverConfiguration> ReadConfigurationFromFundamentals(Tenant tenant)
        {
            var configuration = await _configurationsManager.GetConfigurationForAsync(tenant);
            ConfigurationCache.Add(tenant, configuration);
            return configuration;
        }

        private ILeverConfiguration ReadConfigurationFromAppSettings()
        {
            LeverConfiguration configuration;
            if (ServiceName == "logging")
            {
                //How to handle logging configuration?
                var loggingConnectionString = ConfigurationManager.AppSettings["Logging.ConnectionString"];
                var queueName = ConfigurationManager.AppSettings["Logging.QueueName"];
                var version = ConfigurationManager.AppSettings["Logging.Version"];

                InternalContract.RequireNotNullOrWhiteSpace(loggingConnectionString, nameof(loggingConnectionString), $"Missing mandatory appSetting 'Logging.ConnectionString'");
                InternalContract.RequireNotNullOrWhiteSpace(queueName, nameof(queueName), $"Missing mandatory appSetting 'Logging.QueueName'");
                InternalContract.RequireNotNullOrWhiteSpace(version, nameof(version), $"Missing mandatory appSetting 'Logging.Version'");

                var loggingConfiguration = new LoggingConfiguration(loggingConnectionString, queueName, version);
                var jObject = JObject.FromObject(loggingConfiguration);
                configuration = new LeverConfiguration(ServiceTenant, ServiceName, jObject);
            }
            else
            {
                configuration = new LeverConfiguration(ServiceTenant, ServiceName, new JObject());
            }

            ConfigurationCache.Add(ServiceTenant, configuration);
            return configuration;
        }

        /// <inheritdoc />
        public async Task<ILeverConfiguration> GetConfigurationAsync()
        {
            return await GetConfigurationForAsync(ServiceTenant);
        }

        ///// <inheritdoc />
        //public async Task<ILeverConfiguration> GetConfigurationAsync(string vaultUrl = null, List<string> keyVaultSettings = null)
        //{
        //    var configuration = ConfigurationCache.Get(ServiceTenant);
        //    if (configuration != null) return configuration;

        //    var jObject = new JObject();
        //    foreach (var setting in settings)
        //    {
        //        jObject.Add(ConfigurationManager.AppSettings[setting]);
        //    }

        //    if (keyVaultSettings != null && keyVaultSettings.Any())
        //    {
        //        var keyVaultProvider = new KeyVaultSecretProvider();
        //        await keyVaultProvider.UpdateAppSettingsAsync(vaultUrl, keyVaultSettings);

        //        foreach (var setting in keyVaultSettings)
        //        {
        //            jObject.Add(ConfigurationManager.AppSettings[setting]);
        //        }
        //    }

        //    configuration = new LeverConfiguration(ServiceTenant, ServiceName, jObject);
        //    ConfigurationCache.Add(ServiceTenant, configuration);
        //    return configuration;
        //}
    }
}
