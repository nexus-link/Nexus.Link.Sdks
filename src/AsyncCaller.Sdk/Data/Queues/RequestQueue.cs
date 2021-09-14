using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Sdk.Data.Models;
using Nexus.Link.AsyncCaller.Sdk.Storage.Queue;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.AsyncCaller.Sdk.Data.Queues
{
    public class RequestQueue : IRequestQueue
    {
        private readonly IQueue _queue;


        // We must always have at least this much time until deadline to enter a new message
        private static readonly TimeSpan MinimumBuffer = TimeSpan.FromSeconds(10);
        // There is a maximum wait time that Azure Storage Queue can accept for InitialVisibilityDelay: "The argument 'initialVisibilityDelay' is larger than maximum of '6.23:59:59"
        private static readonly TimeSpan MaximumSpanToWaitForAzureStorageQueue = TimeSpan.FromDays(7)-TimeSpan.FromSeconds(1);
        private static readonly Random Random;

        [Obsolete("Use the constructor with IQueue and string.")]
        // ReSharper disable once UnusedParameter.Local
        public RequestQueue(Tenant tenant, IQueue queue, string name) :this(queue, name)
        {
            
        }

        public RequestQueue(IQueue queue, string name)
        {
            _queue = queue;
            _queue.MaybeCreateAndConnect(name);
        }

static RequestQueue()
        {
            Random = new Random();
        }

        public IQueue GetQueue()
        {
            return _queue;
        }

        public async Task<string> EnqueueAsync(RawRequestEnvelope rawRequestEnvelope, TimeSpan? timeSpanToWait = null, CancellationToken cancellationToken = default)
        {
            var rest = timeSpanToWait == null ? "" : $", {timeSpanToWait}";
            Log.LogInformation($"EnqueueAsync({rawRequestEnvelope}{rest})");
            await _queue.AddMessageAsync(rawRequestEnvelope.Serialize(), timeSpanToWait, cancellationToken);
            return rawRequestEnvelope.RawRequest.Id;
        }

        /// <summary>
        /// Put the <paramref name="rawRequestEnvelope"/> again on the queue.
        /// </summary>
        /// <param name="rawRequestEnvelope">The request envelope to put on the queue.</param>
        /// <param name="latestAttemptAt">The time for the latest attempt. Null if this is a 
        ///     retry because we need to wait longer until next actual call.</param>
        /// <param name="cancellationToken"></param>
        public async Task RequeueAsync(RawRequestEnvelope rawRequestEnvelope, DateTimeOffset? latestAttemptAt = null, CancellationToken cancellationToken = default)
        {
            if (latestAttemptAt != null) rawRequestEnvelope.Attempts++;
            rawRequestEnvelope.LatestAttemptAt = latestAttemptAt ?? rawRequestEnvelope.LatestAttemptAt;
            var timeSpanToWait = RetryDelay(rawRequestEnvelope.Attempts, MaximumSpanToWaitForAzureStorageQueue);
            var now = DateTimeOffset.Now;
            rawRequestEnvelope.NextAttemptAt = now.Add(timeSpanToWait);
            await EnqueueAsync(rawRequestEnvelope, timeSpanToWait, cancellationToken);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await _queue.ClearAsync(cancellationToken);
        }

        // TODO: Is this a good algorithm for calculating how long time we should wait until we retry again?
        private static TimeSpan RetryDelay(int attempts, TimeSpan maxDelay)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, attempts, nameof(attempts));
            InternalContract.RequireGreaterThanOrEqualTo(TimeSpan.FromSeconds(1), maxDelay, nameof(maxDelay));
            // Random delay (factor 4 for the first hour, then factor 2)
            var factor = attempts < 7 ? 4 : 2;
            // Protect the 2^power to overflow the capacity of int, by maximizing the final value to be around a year in seconds.
            var power = Math.Min(attempts, 24);
            var lowerEnd = (int) Math.Pow(factor, power);
            var upperEnd = lowerEnd * factor;
            var delayInSeconds = Random.Next(lowerEnd, upperEnd);
            var delayAsTimeSpan = TimeSpan.FromSeconds(delayInSeconds);
            if (delayAsTimeSpan > maxDelay) delayAsTimeSpan = maxDelay;
            return delayAsTimeSpan;
        }

        /// <inheritdoc />
        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            if (_queue != null) return await _queue.GetResourceHealthAsync(tenant, cancellationToken);
            var response = new HealthResponse("RequestQueue")
            {
                Status = HealthResponse.StatusEnum.Error,
                Message = "GetQueue does not exist"
            };
            return response;
        }

        public async Task<HealthInfo> GetResourceHealth2Async(Tenant tenant, CancellationToken cancellationToken = default)
        {
            if (_queue != null) return await _queue.GetResourceHealth2Async(tenant, cancellationToken);
            var response = new HealthInfo("RequestQueue")
            {
                Status = HealthInfo.StatusEnum.Error,
                Message = "GetQueue does not exist"
            };
            return response;
        }
    }

}