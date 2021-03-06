﻿using System;
using System.Configuration;
using System.Runtime.Caching;
using Nexus.Link.AsyncCaller.Sdk.Data.Queues;
using Nexus.Link.AsyncCaller.Sdk.Storage.Azure.Queue;
using Nexus.Link.AsyncCaller.Sdk.Storage.Memory.Queue;
using Nexus.Link.AsyncCaller.Sdk.Storage.Queue;
using Nexus.Link.Libraries.Core.Decoupling;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.AsyncCaller.Sdk.Dispatcher.Helpers
{
    public static class RequestQueueHelper
    {
        public const string DefaultQueueName = "async-caller";
        public const string MultipleQueueNameInterfix = "-";
        public const string MemoryQueueConnectionString = "UseMemoryQueue=true";

        private static readonly ObjectCache RequestQueueCache = MemoryCache.Default;

        public static IRequestQueue GetRequestQueueOrThrow(Tenant tenant, ILeverConfiguration config, int? priority)
        {
            var cacheKey = $"RequestQueue|{tenant.Organization}|{tenant.Environment}|{priority}";
            if (RequestQueueCache[cacheKey] is RequestQueue requestQueue) return requestQueue;

            var queueName = FindQueueName(priority, config);
            requestQueue = MaybeUseMemoryQueue(queueName);
            if (requestQueue != null) return requestQueue;

            var connectionString = config.MandatoryValue<string>("ConnectionString");
            var queue = CreateQueue(connectionString, queueName);

            requestQueue = new RequestQueue(queue, queueName);
            RequestQueueCache.Set(cacheKey, requestQueue, DateTimeOffset.Now.AddMinutes(15));

            return requestQueue;
        }

        private static IQueue CreateQueue(string connectionString, string queueName)
        {
            // Memory queue support for local development and unit tests
            if (connectionString == MemoryQueueConnectionString)
            {
                return MemoryQueue.Instance(queueName);
            }

            return new AzureStorageQueue(connectionString);
        }

        private static string FindQueueName(int? priority, ILeverConfiguration config)
        {
            var schemaVersion = config.Value<int?>(nameof(AnonymousSchema.SchemaVersion));
            string queueName;
            if (schemaVersion == null || schemaVersion < 1) queueName = config.MandatoryValue<string>("QueueName");
            else queueName = DefaultQueueName;
            return priority.HasValue ? $"{queueName}{MultipleQueueNameInterfix}{priority}" : queueName;
        }

        private static RequestQueue MaybeUseMemoryQueue(string queueName)
        {
            // Local development support
            var appSetting = ConfigurationManager.AppSettings["UseMemoryQueue"];
            var useMemoryQueue = appSetting != null && bool.Parse(appSetting);
            if (useMemoryQueue)
            {
                var memoryQueue = MemoryQueue.Instance(queueName);
                return new RequestQueue(memoryQueue, memoryQueue.QueueName);
            }
            return null;
        }
    }
}