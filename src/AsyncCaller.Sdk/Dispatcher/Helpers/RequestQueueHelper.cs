using System;
using System.Configuration;
using System.Runtime.Caching;
using Nexus.Link.Libraries.Core.Decoupling;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Xlent.Lever.AsyncCaller.Data.Queues;
using Xlent.Lever.AsyncCaller.Storage.Azure.Queue;
using Xlent.Lever.AsyncCaller.Storage.Memory.Queue;
using Xlent.Lever.AsyncCaller.Storage.Queue;

namespace Nexus.Link.AsyncCaller.Dispatcher.Helpers
{
    public static class RequestQueueHelper
    {
        public const string DefaultQueueName = "async-caller";
        public const string PriorityQueueNameInterfix = "-priority-";
        public const string MemoryQueueConnectionString = "UseMemoryQueue=true";

        private static readonly ObjectCache RequestQueueCache = MemoryCache.Default;

        public static IRequestQueue GetRequestQueueOrThrow(Tenant tenant, ILeverConfiguration config, int? priority)
        {
            var cacheKey = $"RequestQueue|{tenant.Organization}|{tenant.Environment}|{priority}";
            if (RequestQueueCache[cacheKey] is RequestQueue requestQueue) return requestQueue;

            requestQueue = MaybeUseMemoryQueue();
            if (requestQueue != null) return requestQueue;

            var connectionString = config.MandatoryValue<string>("ConnectionString");
            var queueName = FindQueueName(priority, config);
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
            return priority.HasValue ? $"{queueName}{PriorityQueueNameInterfix}{priority}" : queueName;
        }

        private static RequestQueue MaybeUseMemoryQueue()
        {
            // Local development support
            var appSetting = ConfigurationManager.AppSettings["UseMemoryQueue"];
            var useMemoryQueue = appSetting != null && bool.Parse(appSetting);
            if (useMemoryQueue)
            {
                return new RequestQueue(MemoryQueue.Instance(), MemoryQueue.Instance().QueueName);
            }
            return null;
        }
    }
}