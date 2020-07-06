using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Core.Queue.Model;
using System.Threading.Tasks;
using Nexus.Link.Configurations.Sdk;

namespace Nexus.Link.Logger.Sdk.Helpers
{
    public class LogQueueHelper<T> : ILogQueueHelper<T>
    {
        private readonly ILeverServiceConfiguration _loggingServiceConfiguration;
        private readonly LoggingConfiguration _loggingConfiguration;

        public LogQueueHelper()
        {
        }

        public LogQueueHelper(ILeverServiceConfiguration loggingServiceConfiguration, LoggingConfiguration loggingConfiguration)
        {
            _loggingServiceConfiguration = loggingServiceConfiguration;
            _loggingConfiguration = loggingConfiguration;
        }

        public LogQueueHelper(ILeverServiceConfiguration loggingServiceConfiguration)
        {
            _loggingServiceConfiguration = loggingServiceConfiguration;
        }

        public async Task<(bool HasStorageQueue, IWritableQueue<T> WritableQueue)> TryGetQueueAsync(Tenant tenant)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireValidated(tenant, nameof(tenant));

            var service = FulcrumApplication.Setup.Name;
            var serviceTenant = FulcrumApplication.Setup.Tenant;

            var useLoggingConfiguration = serviceTenant.Equals(tenant) && _loggingConfiguration != null;

            if (useLoggingConfiguration)
            {
                return await TryGetQueueFromLoggingConfiguration(service, tenant);
            }
            else
            {
                return await TryGetQueueFromFundamentals(service, tenant);
            }
        }

        private async Task<(bool HasStorageQueue, IWritableQueue<T> WritableQueue)> TryGetQueueFromLoggingConfiguration(string service, Tenant tenant)
        {
            if (_loggingConfiguration == null)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Warning,
                    $"Will use serviceconfiguration due to ILeverServiceConfiguration was not provided.");

                return (AzureStorageQueueIsCreated.No, null);
            }

            var connectionString = _loggingConfiguration?.ConnectionString;
            if (connectionString == null)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Warning,
                    $"Configuration for service \"Logging\" for tenant {tenant} must have setting for LoggerConnectionString");
                return (AzureStorageQueueIsCreated.No, null);
            }

            var queueName = _loggingConfiguration?.QueueName;
            if (queueName == null)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Warning,
                    $"Configuration for service \"{service}\" for tenant {tenant} must have setting for QueueName");
                return (AzureStorageQueueIsCreated.No, null);
            }

            return (AzureStorageQueueIsCreated.Yes, new TruncatingAzureStorageQueue<T>(connectionString, queueName));
        }

        private async Task<(bool HasStorageQueue, IWritableQueue<T> WritableQueue)> TryGetQueueFromFundamentals(string service, Tenant tenant)
        {
            if (_loggingServiceConfiguration == null)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Warning,
                    $"Will use serviceconfiguration due to ILeverServiceConfiguration was not provided.");

                return (AzureStorageQueueIsCreated.No, null);
            }

            var tenantLoggingConfiguration = await _loggingServiceConfiguration.GetConfigurationForAsync(tenant);

            var connectionString = tenantLoggingConfiguration?.Value<string>("LoggerConnectionString");
            if (connectionString == null)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Warning,
                    $"Configuration for service \"Logging\" for tenant {tenant} must have setting for LoggerConnectionString");
                return (AzureStorageQueueIsCreated.No, null);
            }

            var queueName = tenantLoggingConfiguration?.Value<string>("QueueName");
            if (queueName == null)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Warning,
                    $"Configuration for service \"{service}\" for tenant {tenant} must have setting for QueueName");
                return (AzureStorageQueueIsCreated.No, null);
            }

            return (AzureStorageQueueIsCreated.Yes, new TruncatingAzureStorageQueue<T>(connectionString, queueName));
        }

        private static class AzureStorageQueueIsCreated
        {
            public const bool No = false;
            public const bool Yes = true;
        }
    }
}