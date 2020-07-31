using System;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;

namespace Nexus.Link.Commands.Sdk
{
    public class NexusCommandsOptions
    {
        /// <summary>
        /// In development mode, you can use a memory storage
        /// </summary>
        public bool UseHangfireMemoryStorage { get; set; }

        /// <summary>
        /// The background process is backed by Hangfire; this is the connection string to a database to use for it
        /// </summary>
        public string HangfireSqlConnectionString { get; set; }

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

    }
}