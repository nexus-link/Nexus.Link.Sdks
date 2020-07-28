using Nexus.Link.Configurations.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Nexus.Link.AsyncCaller.Common.Configuration
{
    public static class ConfigurationHandler
    {
        public static LeverServiceConfiguration LeverServiceConfiguration { get; private set; }

        public static LeverServiceConfiguration LoggingConfiguration { get; private set; }

        static ConfigurationHandler()
        {
            ServiceTenant = new Tenant(ConfigurationManager.AppSettings["Organization"], ConfigurationManager.AppSettings["Environment"]);
        }

        public static void ONLY_FOR_TESTING_SetServiceConfiguration(ILeverServiceConfiguration serviceConfiguration)
        {
            ServiceConfiguration = serviceConfiguration;
        }

        public static Tenant ServiceTenant { get; }

        private static ILeverServiceConfiguration _serviceConfiguration;

        public static ILeverServiceConfiguration ServiceConfiguration
        {
            get
            {
                if (_serviceConfiguration != null) return _serviceConfiguration;

                var authenticationServiceUri = ConfigurationManager.AppSettings["Authentication.Url"];
                var configurationsServiceUrl = ConfigurationManager.AppSettings["ConfigurationsBaseUrl"];
                var serviceCredentials = new AuthenticationCredentials
                {
                    ClientId = ConfigurationManager.AppSettings["Authentication.ClientId"],
                    ClientSecret = ConfigurationManager.AppSettings["Authentication.ClientSecret"]
                };
                LeverServiceConfiguration = new LeverServiceConfiguration(ServiceTenant, "AsyncCaller", authenticationServiceUri, serviceCredentials, configurationsServiceUrl);

                try
                {
                    LoggingConfiguration = new LeverServiceConfiguration(ServiceTenant, "logging", authenticationServiceUri, serviceCredentials, configurationsServiceUrl);
                }
                catch (Exception)
                {
                    // Virtuell tjänst (kanske) inte konfigurerad?
                }

                _serviceConfiguration = LeverServiceConfiguration;
                return _serviceConfiguration;
            }
            private set => _serviceConfiguration = value;
        }

        public static async Task<ILeverConfiguration> GetConfigurationForAsync(Tenant tenant)
        {
            return await ServiceConfiguration.GetConfigurationForAsync(tenant);
        }

        public static async Task<ILeverConfiguration> GetConfigurationAsync()
        {
            return await ServiceConfiguration.GetConfigurationAsync();
        }

        public static async Task<TimeSpan> GetDefaultDeadlineTimeSpanAsync(Tenant tenant)
        {
            TimeSpan timeSpan;
            try
            {
                var config = await GetConfigurationForAsync(tenant);
                var defaultDeadlineTimespanInSeconds = config.Value<double?>("DefaultDeadlineTimeSpanInSeconds");
                if (defaultDeadlineTimespanInSeconds != null && defaultDeadlineTimespanInSeconds.Value > 0)
                {
                    timeSpan = TimeSpan.FromSeconds(defaultDeadlineTimespanInSeconds.Value);
                }
                else
                {
                    if (!FulcrumApplication.IsInProductionOrProductionSimulation)
                    {
                        Log.LogWarning(
                            $"Missing configuration for DefaultDeadlineTimeSpanInSeconds. The fallback plan is to use a deadline time span of 48 hours.");
                    }

                    timeSpan = TimeSpan.FromHours(48);
                }
            }
            catch (Exception e)
            {
                Log.LogError($"Could not get the default deadline time for new messages. The fallback plan is to use a deadline time span of 7 days. Exception message: {e.Message}");
                timeSpan = TimeSpan.FromDays(7);
            }

            return timeSpan;
        }
    }
}