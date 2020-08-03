using System;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Commands.Sdk
{
    public class NexusCommandsOptions : IValidatable
    {
        /// <summary>
        /// The name of the service to check commands for
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// The id of the instance to check commands for
        /// </summary>
        public string InstanceId { get; }

        /// <summary>
        /// In development mode, you can use a memory storage
        /// </summary>
        public bool UseHangfireMemoryStorage { get; set; }

        /// <summary>
        /// The background process is backed by Hangfire; this is the connection string to a database to use for it
        /// </summary>
        public string HangfireSqlConnectionString { get; set; }


        public NexusCommandsOptions(string serviceName, string instanceId)
        {
            ServiceName = serviceName;
            InstanceId = instanceId;
        }

        public SqlServerStorageOptions SqlServerStorageOptions { get; set; } = new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromSeconds(60),
            SlidingInvisibilityTimeout = TimeSpan.FromSeconds(60),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            JobExpirationCheckInterval = TimeSpan.FromSeconds(1),
            // TODO:
        };

        public MemoryStorageOptions MemoryStorageOptions { get; set; } = new MemoryStorageOptions
        {
            FetchNextJobTimeout = TimeSpan.FromSeconds(1),
            JobExpirationCheckInterval = TimeSpan.FromSeconds(1),
            CountersAggregateInterval = TimeSpan.FromSeconds(1),
            // TODO
        };

        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(ServiceName, nameof(ServiceName), errorLocation);
            FulcrumValidate.IsNotNull(InstanceId, nameof(InstanceId), errorLocation);
            if (UseHangfireMemoryStorage)
            {
                FulcrumValidate.IsNotNull(MemoryStorageOptions, nameof(MemoryStorageOptions), errorLocation);
            }
            else
            {
                FulcrumValidate.IsNotNullOrWhiteSpace(HangfireSqlConnectionString, nameof(HangfireSqlConnectionString), errorLocation);
                FulcrumValidate.IsNotNull(SqlServerStorageOptions, nameof(SqlServerStorageOptions), errorLocation);
            }
        }
    }
}