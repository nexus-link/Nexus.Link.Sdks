using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Nexus.Link.AsyncCaller.Sdk.Storage.Queue;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Threads;

namespace Nexus.Link.AsyncCaller.Sdk.Storage.Azure.Queue
{
    // TODO: We should probably use our Libraries variant rather than this one. The one in Libraries are based on Storage.V12, this is based on Storage.V11
    public class AzureStorageQueue : IQueue
    {
        private static readonly string Namespace = typeof(AzureStorageQueue).Namespace;
        private CloudQueue _queue;
        private readonly CloudQueueClient _queueClient;
        private readonly Uri _queueEndpoint;

        public string QueueName
        {
            get
            {
                InternalContract.Require(_queue != null, "There is no connection to a queue. Please call Connect() first.");
                return _queue?.Name;
            }
        }

        public AzureStorageQueue(string connectionString)
        {
            InternalContract.RequireNotNull(connectionString, nameof(connectionString));
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            _queueEndpoint = storageAccount.QueueEndpoint;
            _queueClient = storageAccount.CreateCloudQueueClient();
            FulcrumAssert.IsNotNull(_queueClient, $"{Namespace}: F0EF67D4-9F1A-426B-B2C6-41557D3CC947", "Could not create a cloud queue client.");
        }

        //public AzureStorageQueue(CloudQueue queue)
        //{
        //    InternalContract.RequireNotNull(queue, nameof(queue));
        //    _queue = queue;
        //    _queueClient = _queue.ServiceClient;
        //}

        public bool MaybeCreateAndConnect(string name)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name), $"{nameof(MaybeCreateAndConnect)}: 31CD123B-1A1A-4DEF-B8CA-3743FAB19C55");
            if (_queue != null) return false;
            FulcrumAssert.IsNotNull(_queueClient, $"{Namespace}: 5DD337BE-B1A6-493D-95D5-B05C9717F14A", $"Expected to have a queue client ready for queue {name}.");
            _queue = _queueClient.GetQueueReference(name);
            FulcrumAssert.IsNotNull(_queue, $"{Namespace}: 117B4E2E-C94C-462D-8A2B-130805EFE69B", $"Failed to create a queue reference to {name}");
            try
            {
                var created = ThreadHelper.CallAsyncFromSync(async () => await _queue.CreateIfNotExistsAsync());
                return created;
            }
            catch (Exception e)
            {
                throw new FulcrumResourceException($"Could not connect to queue '{name}' on storage '{_queueEndpoint}': {e.Message}", e);
            }
        }

        public async Task AddMessageAsync(string message, TimeSpan? timeSpanToWait, CancellationToken cancellationToken = default)
        {
            FulcrumAssert.IsNotNull(_queue, $"{Namespace}: 99A13992-135C-4624-AED7-1FEA9D02C925", "There is no connection to a queue. Please call Connect() before adding messages to the queue.");
            await _queue.AddMessageAsync(new CloudQueueMessage(message), null, timeSpanToWait, null, null, cancellationToken);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await _queue.ClearAsync(null, null, cancellationToken);
        }

        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            var response = new HealthResponse("AzureStorageQueue");
            if (_queue == null)
            {
                response.Status = HealthResponse.StatusEnum.Error;
                response.Message = "AzureStorageQueue was not found.";
            }
            return await Task.FromResult(response);
        }

        public async Task<HealthInfo> GetResourceHealth2Async(Tenant tenant, CancellationToken cancellationToken = default)
        {
            var response = new HealthInfo("AzureStorageQueue")
            {
                Status = HealthInfo.StatusEnum.Ok,
                Message = "Ok"
            };
            if (_queue == null)
            {
                response.Status = HealthInfo.StatusEnum.Error;
                response.Message = "AzureStorageQueue was not found.";
            }
            return await Task.FromResult(response);
        }
    }
}
