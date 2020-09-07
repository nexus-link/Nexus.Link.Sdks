using System;
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

        public IQueue GetQueue()
        {
            return _queue;
        }

        public async Task<string> EnqueueAsync(RawRequestEnvelope rawRequestEnvelope, TimeSpan? timeSpanToWait = null)
        {
            var rest = timeSpanToWait == null ? "" : $", {timeSpanToWait}";
            Log.LogInformation($"EnqueueAsync({rawRequestEnvelope}{rest})");
            await _queue.AddMessageAsync(rawRequestEnvelope.Serialize(), timeSpanToWait);
            return rawRequestEnvelope.RawRequest.Id;
        }

        /// <summary>
        /// Put the <paramref name="rawRequestEnvelope"/> again on the queue.
        /// </summary>
        /// <param name="rawRequestEnvelope">The request envelope to put on the queue.</param>
        /// <param name="latestAttemptAt">The time for the latest attempt. Null if this is a 
        ///     retry because we need to wait longer until next actual call.</param>
        public async Task RequeueAsync(RawRequestEnvelope rawRequestEnvelope, DateTimeOffset? latestAttemptAt = null)
        {
            if (latestAttemptAt != null) rawRequestEnvelope.Attempts++;
            rawRequestEnvelope.LatestAttemptAt = latestAttemptAt ?? rawRequestEnvelope.LatestAttemptAt;
            var timeSpanToWait = RetryDelay(rawRequestEnvelope.Attempts);
            var now = DateTimeOffset.Now;
            rawRequestEnvelope.NextAttemptAt = now.Add(timeSpanToWait);
            await EnqueueAsync(rawRequestEnvelope, timeSpanToWait);
        }

        public async Task ClearAsync()
        {
            await _queue.ClearAsync();
        }

        // TODO: Is this a good algorithm for calculating how long time we should wait until we retry again?
        private static TimeSpan RetryDelay(int attempts)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, attempts, nameof(attempts));
            // Random delay (factor 4 for the first hour, then factor 2)
            var factor = attempts < 7 ? 4 : 2;
            var random = new Random();
            var lowerEnd = (int) Math.Pow(factor, attempts);
            var upperEnd = lowerEnd * factor;
            var delayInSeconds = random.Next(lowerEnd, upperEnd);
            var delayInSecondsAsTimeSpan = TimeSpan.FromSeconds(delayInSeconds);
            return delayInSecondsAsTimeSpan;
        }

        /// <inheritdoc />
        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant)
        {
            if (_queue != null) return await _queue.GetResourceHealthAsync(tenant);
            var response = new HealthResponse("RequestQueue")
            {
                Status = HealthResponse.StatusEnum.Error,
                Message = "GetQueue does not exist"
            };
            return response;
        }

        public async Task<HealthInfo> GetResourceHealth2Async(Tenant tenant)
        {
            if (_queue != null) return await _queue.GetResourceHealth2Async(tenant);
            var response = new HealthInfo("RequestQueue")
            {
                Status = HealthInfo.StatusEnum.Error,
                Message = "GetQueue does not exist"
            };
            return response;
        }
    }

}