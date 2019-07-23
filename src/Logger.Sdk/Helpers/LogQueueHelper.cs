using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Queue.Model;

namespace Nexus.Link.Logger.Sdk.Helpers
{
    public class LogQueueHelper<T> : ILogQueueHelper<T>
    {
        // TODO: TryGet för att falla tillbaka på en RestClient
        public bool TryGetQueue(Tenant tenant, out IWritableQueue<T> queue)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireValidated(tenant, nameof(tenant));

            queue = null;
            var service = FulcrumApplication.Setup.Name;

            var config = FulcrumApplication.Context.LeverConfiguration;
            if (config == null)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Warning,
                    $"Will use serviceconfiguration due to Configuration for service \"{service}\" was not found for tenant {tenant}.");

                return AzureStorageQueueIsCreated.No;
                //throw new FulcrumBusinessRuleException($"Configuration for service \"{service}\" was not found for tenant {tenant}.");
            }

            var connectionString = config.Value<string>("LoggerConnectionString");
            if (connectionString == null)
            {
                return AzureStorageQueueIsCreated.No;
                //throw new FulcrumBusinessRuleException(
                //    $"Configuration for service \"{service}\" for tenant {tenant} must have setting for LoggerConnectionString");
            }

            var queueName = config.Value<string>("QueueName");
            if (queueName == null)
            {
                LogHelper.FallbackSafeLog(LogSeverityLevel.Warning,
                    $"Configuration for service \"{service}\" for tenant {tenant} must have setting for QueueName");
                return AzureStorageQueueIsCreated.No;
                //throw new FulcrumBusinessRuleException(
                //    $"Configuration for service \"{service}\" for tenant {tenant} must have setting for QueueName");
            }

            queue = new TruncatingAzureStorageQueue<T>(connectionString, queueName);
            return AzureStorageQueueIsCreated.Yes;
        }

        private static class AzureStorageQueueIsCreated
        {
            public const bool No = false;
            public const bool Yes = true;
        }
    }
}