using System;
using System.Threading.Tasks;
using Xlent.Lever.AsyncCaller.Data.Models;
using Xlent.Lever.AsyncCaller.Storage.Queue;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Xlent.Lever.AsyncCaller.Data.Queues
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

        public async Task<string> EnqueueAsync(RequestEnvelope requestEnvelope, TimeSpan? timeSpanToWait = null)
        {
            var rest = timeSpanToWait == null ? "" : $", {timeSpanToWait}";
            Log.LogInformation($"EnqueueAsync({requestEnvelope}{rest})");
            await _queue.AddMessageAsync(requestEnvelope.Serialize(), timeSpanToWait);
            return requestEnvelope.RawRequest.Id;
        }

        /// <summary>
        /// Put the <paramref name="requestEnvelope"/> again on the queue.
        /// </summary>
        /// <param name="requestEnvelope">The request envelope to put on the queue.</param>
        /// <param name="latestAttemptAt">The time for the latest attempt. Null if this is a 
        ///     retry because we need to wait longer until next actual call.</param>
        public async Task RequeueAsync(RequestEnvelope requestEnvelope, DateTimeOffset? latestAttemptAt = null)
        {
            if (latestAttemptAt != null) requestEnvelope.Attempts++;
            requestEnvelope.LatestAttemptAt = latestAttemptAt ?? requestEnvelope.LatestAttemptAt;
            var timeSpanToWait = RetryDelay(requestEnvelope.Attempts);
            var now = DateTimeOffset.Now;
            requestEnvelope.NextAttemptAt = now.Add(timeSpanToWait);
            await EnqueueAsync(requestEnvelope, timeSpanToWait);
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