using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Logging;
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
        public const string PrioritizedQueuesSetting = "PrioritizedQueues";
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
            string queueName;
            if (config.Value<string>("DistributionVersion") == "2")
            {
                queueName = FindQueueNameDistributionVersion2(priority);
            }
            else
            {
                queueName = FindQueueNameDistributionVersion1(priority, config);
            }

            return queueName;
        }

        private static string FindQueueNameDistributionVersion1(int? priority, ILeverConfiguration config)
        {
            string queueName = null;
            if (priority.HasValue)
            {
                // When using priority queue, there should be lever configuration for it
                var prioQueues = config.Value<Dictionary<int, string>>(PrioritizedQueuesSetting);
                if (prioQueues != null && prioQueues.ContainsKey((int)priority))
                {
                    queueName = prioQueues[(int)priority];
                }
                else
                {
                    Log.LogWarning($"Priority queue '{priority}' was not found in setting {PrioritizedQueuesSetting}: {JsonConvert.SerializeObject(prioQueues)}. Reverting to default queue.");
                }
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                queueName = config.MandatoryValue<string>("QueueName");
            }

            return queueName;
        }

        private static string FindQueueNameDistributionVersion2(int? priority)
        {
            if (!priority.HasValue)
            {
                return "async-caller-standard-queue";
            }

            var zeroPadding = priority < 10 ? "0" : "";
            return $"async-caller-priority{zeroPadding}{priority}-queue";

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