using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Sdk.Storage.Queue;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.AsyncCaller.Sdk.Storage.Memory.Queue
{
    /// <summary>
    /// A generic interface for adding strings to a queue.
    /// </summary>
    public class MemoryQueue : IQueue
    {
        private static readonly string Namespace = typeof(MemoryQueue).Namespace;
        private static readonly object LockObject = new object();
        private Queue<string> _queue;
        private const string SingletonQueueName = "SingletonMemoryQueue";
        private static readonly Dictionary<string, MemoryQueue> Instances = new Dictionary<string, MemoryQueue>();

        private MemoryQueue()
        {
        }

        public static MemoryQueue Instance(string queueName = SingletonQueueName)
        {
            MemoryQueue memoryQueue = null;
            if (Instances.ContainsKey(queueName)) memoryQueue = Instances[queueName];
            if (memoryQueue != null)
            {
                Log.LogVerbose($"Returning existing memory queue: {memoryQueue}");
                return memoryQueue;
            }

            lock (LockObject)
            {
                if (Instances.ContainsKey(queueName)) memoryQueue = Instances[queueName];
                if (memoryQueue != null) return memoryQueue;

                memoryQueue = new MemoryQueue();
                memoryQueue.MaybeCreateAndConnect(queueName);
                Instances.Add(queueName, memoryQueue);
                Log.LogVerbose($"Created memory queue: {memoryQueue}");
                return memoryQueue;
            }
        }

        public string QueueName { get; private set; }

        public bool MaybeCreateAndConnect(string name)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            lock (LockObject)
            {
                if (name == QueueName) return false;
                InternalContract.Require(_queue == null, $"There can only be one memory queue, and there already exists one (named \"{QueueName}\").");
                QueueName = name;
                _queue = new Queue<string>();
            }
            return true;
        }

        public async Task AddMessageAsync(string message, TimeSpan? timeSpanToWait = null, CancellationToken cancellationToken = default)
        {
            lock (LockObject)
            {
                FulcrumAssert.IsNotNull(_queue, $"{Namespace}: 9BD616FD-3867-453C-93C6-13B4767A6FE5", $"Expected the queue ({QueueName}) to exist. Did you forget to call MaybeCreateAndConnect()?");
                Log.LogVerbose($"Add message {message}");
                _queue.Enqueue(message);
            }
            await Task.Yield();
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            lock (LockObject)
            {
                if (_queue == null) return;
                Log.LogVerbose($"Clearing memory queue: {this}");
                _queue.Clear();
            }
            await Task.Yield();
        }

        public string GetOneMessageNoBlock()
        {
            lock (LockObject)
            {
                Log.LogVerbose($"Get one message of: {_queue.Count}; {DateTimeOffset.Now:O}");
                return _queue.Any() ? _queue.Dequeue() : null;
            }
        }

        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new HealthResponse("MemoryQueue"));
        }

        public async Task<HealthInfo> GetResourceHealth2Async(Tenant tenant, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new HealthInfo("MemoryQueue") { Status = HealthInfo.StatusEnum.Ok, Message = "Ok" });
        }

        public override string ToString()
        {
            return $"[name: {QueueName}, item count: {_queue.Count}]";
        }
    }
}
