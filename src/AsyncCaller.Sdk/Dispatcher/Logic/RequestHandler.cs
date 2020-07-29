using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Dispatcher.Exceptions;
using Nexus.Link.AsyncCaller.Dispatcher.Helpers;
using Nexus.Link.AsyncCaller.Dispatcher.Models;
using Nexus.Link.AsyncCaller.Sdk.Common.Helpers;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Logging;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Xlent.Lever.AsyncCaller.Data.Queues;
using RequestEnvelope = Nexus.Link.AsyncCaller.Dispatcher.Models.RequestEnvelope;

namespace Nexus.Link.AsyncCaller.Dispatcher.Logic
{
    public class RequestHandler
    {
        private readonly IRequestQueue _requestQueue;
        private readonly RequestEnvelope _envelope;
        private readonly IHttpClient _sender;
        private readonly TimeSpan _defaultDeadlineInSeconds;
        private readonly Xlent.Lever.AsyncCaller.Data.Models.RequestEnvelope _originalRequestEnvelope;

        public RequestHandler(IHttpClient sender, Tenant tenant, ILeverConfiguration config, Xlent.Lever.AsyncCaller.Data.Models.RequestEnvelope requestEnvelope)
        {
            _sender = sender;
            var priority = requestEnvelope?.RawRequest?.Priority;
            _requestQueue = RequestQueueHelper.GetRequestQueueOrThrow(tenant, config, priority);
            _originalRequestEnvelope = requestEnvelope;
            _defaultDeadlineInSeconds = ConfigurationHelper.GetDefaultDeadlineTimeSpan(config);
            _envelope = ThreadHelper.CallAsyncFromSync(async () => await RequestEnvelope.FromDataAsync(_originalRequestEnvelope, _defaultDeadlineInSeconds));
        }

        public async Task ProcessOneRequestAsync()
        {
            try
            {
                if (IsTimeForNextCall) await SendRequestAndHandleResponseAsync();
                else await PutBackLastInQueueAsync();
            }
            catch (GiveUpException e)
            {
                // TODO: Put on poison queue?
                Log.LogError($"Giving up on envelope ({_envelope}): {e.Message}");
            }
        }

        private async Task SendRequestAndHandleResponseAsync()
        {
            var envelopeAsString = _envelope.ToString();
            try
            {
                if (HasReachedDeadLine) throw new DeadlineReachedException(_envelope.DeadlineAt);
                var response = await _sender.SendAsync(_envelope.Request.CallOut, CancellationToken.None);
                FulcrumAssert.IsNotNull(response, $"Expected to receive a non-null response when making the call for {envelopeAsString}.");
                if (response.IsSuccessStatusCode) await HandleSuccessfulResponseAsync(response);
                else await HandleFailureResponseAsync(response);
            }
            catch (GiveUpException)
            {
                throw;
            }
            catch (DeadlineReachedException e)
            {
                throw new GiveUpException(_envelope, e.Message);
            }
            catch (Exception e)
            {
                if (!(e is FulcrumAssertionFailedException))
                {
                    Log.LogError($"Internal error when making the call for {envelopeAsString}", e);
                }
                // NOTE! This is not an HTTP request that returned a non-OK value, it is an internal error.
                // Could be because we have no internet connection and all sorts of errors.
                // Try to put the item back last on the queue for a later retry.
                var internalErrorResponse = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(e.Message, Encoding.UTF8, "text/plain")
                };
                await HandleFailureResponseAsync(internalErrorResponse, true);
            }
        }

        private async Task HandleSuccessfulResponseAsync(HttpResponseMessage response)
        {
            if (!AcceptsCallback) return;
            var requestEnvelope = await RequestEnvelope.GetResponseAsRequestEnvelopeAsync(_envelope, response, _defaultDeadlineInSeconds);
            var dataEnvelope = await requestEnvelope.ToDataAsync();
            await _requestQueue.EnqueueAsync(dataEnvelope);
        }

        private async Task HandleFailureResponseAsync(HttpResponseMessage response, bool internalError = false)
        {
            if (!HasEarlierResponse || !internalError)
            {
                _envelope.LatestResponse = response;
                _envelope.LatestAttemptAt = DateTimeOffset.Now;
            }

            if (await IsTemporaryFailureAsync(response))
            {
                Log.LogInformation($"TemporaryFailure({response.StatusCode}) -> Put back last in queue '{_requestQueue.GetQueue().QueueName}'");
                await PutBackLastInQueueAsync(DateTimeOffset.Now);
                return;
            }
            if (!AcceptsCallback) throw new GiveUpException(_envelope, $"We received a response  ({await response.ToLogStringAsync()}), but there is no callback address.");
            var requestEnvelope = await RequestEnvelope.GetResponseAsRequestEnvelopeAsync(_envelope, response, _defaultDeadlineInSeconds);
            var dataEnvelope = await requestEnvelope.ToDataAsync();
            await _requestQueue.EnqueueAsync(dataEnvelope);
        }

        private async Task PutBackLastInQueueAsync(DateTimeOffset? latestAttemptAt = null)
        {
            _envelope.Request = await Request.FromDataAsync(_originalRequestEnvelope.RawRequest);
            var dataEnvelope = await _envelope.ToDataAsync();
            await Task.Delay(TimeSpan.FromSeconds(1));
            await _requestQueue.RequeueAsync(dataEnvelope, latestAttemptAt);
        }

        /// <summary>
        /// Check if response message is considered temporary.
        /// 
        /// Strategy for resend based on FulcrumError.sRetryMeaningful and otherwise the http status codes. Http status codes that will trigger a resend are 404, 408, 425, 429, 500 and above, except 510 and 526.
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        private static async Task<bool> IsTemporaryFailureAsync(HttpResponseMessage responseMessage)
        {
            // Get status code and content from response message
            HttpStatusCode statusCode = responseMessage.StatusCode;
            string content = null;

            // Get content if it exists
            if (responseMessage.Content != null)
            {
                await responseMessage.Content?.LoadIntoBufferAsync();
                content = await responseMessage.Content?.ReadAsStringAsync();
            }

            // Handle fulcrum Exception:
            // If response message can be parsed into an fulcrum error and if it has a Type -> use the IsRetryMeaningful attribute
            var fulcrumError = ExceptionConverter.SafeParse<FulcrumError>(content);
            if (fulcrumError?.Type != null)
            {
                return fulcrumError.IsRetryMeaningful;
            }

            // Handle 404: Not Found
            if (statusCode == HttpStatusCode.NotFound) return true;

            // Handle 408: Request Timeout
            if (statusCode == HttpStatusCode.RequestTimeout) return true;

            // Handle 425: Too Early
            if (statusCode == (HttpStatusCode)425) return true;

            // Handle 429: Too Many Requests
            if (statusCode == (HttpStatusCode)429) return true;

            // Handle >=500: Server error responses
            if (statusCode >= HttpStatusCode.InternalServerError)
            {
                // Make exception for code 510: Not Extended
                // and code 526: Invalid SSL Certificate
                if (statusCode != (HttpStatusCode)510 && statusCode != (HttpStatusCode)526) return true;
            }

            // Other response codes are not considered temporary
            return false;
        }

        private bool HasReachedDeadLine => _envelope.DeadlineAt <= DateTimeOffset.Now;
        private bool AcceptsCallback => _envelope.Request.CallBack?.RequestUri != null;
        private bool HasEarlierResponse => _envelope.LatestResponse != null;
        private bool IsTimeForNextCall => _envelope.NextAttemptAt < DateTimeOffset.Now;
    }
}
