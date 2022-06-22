using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Misc.Web.Sdk.Outbound.Options;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Logging;
using Nexus.Link.Libraries.Web.Pipe;

namespace Misc.Web.Sdk.Outbound
{
    /// <summary>
    /// This handler contains all the Nexus Link handlers.
    /// </summary>
    public class NexusLinkHandler : DelegatingHandler
    {
        private readonly NexusLinkHandlerOptions _options;

        public NexusLinkHandler(NexusLinkHandlerOptions options)
        {
            InternalContract.RequireNotNull(options, nameof(options));
            InternalContract.RequireValidated(options, nameof(options));
            _options = options;
        }

        public async Task<HttpResponseMessage> TestSendAsync(HttpRequestMessage request, 
            CustomSendDelegateOptions.SendAsyncMethod sendAsyncDelegate = null,
            CancellationToken cancellationToken = default)
        {
            var oldEnabled = _options.Features.CustomSendDelegate.Enabled;
            var oldSendAsyncDelegate = _options.Features.CustomSendDelegate.SendAsyncDelegate;

            if (sendAsyncDelegate != null)
            {
                _options.Features.CustomSendDelegate.Enabled = true;
                _options.Features.CustomSendDelegate.SendAsyncDelegate = sendAsyncDelegate;
            }

            try
            {
                var result = await SendAsync(request, cancellationToken);
                return result;
            }
            finally
            {
                _options.Features.CustomSendDelegate.Enabled = oldEnabled;
                _options.Features.CustomSendDelegate.SendAsyncDelegate = oldSendAsyncDelegate;
            }
        }

        /// <summary>
        /// Adds a Fulcrum ForwardCorrelationId to the requests before sending it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ForwardHeaders();
            var timer = new Stopwatch();
            HttpResponseMessage response = null;

            timer.Start(); try
            {
                response = await SendRequestAsync();
            }
            catch (Exception e)
            {
                timer.Stop();
                HandleExceptionsAndMaybeThrowNewException(e);
                throw;
            }
            timer.Stop();
            await HandleResponseAsync();
            return response;

            void ForwardHeaders()
            {
                if (_options.Features.ForwardCorrelationId.Enabled)
                {
                    ForwardCorrelationId(request);
                }

                if (_options.Features.ForwardNexusTranslatedUserId.Enabled)
                {
                    ForwardNexusTranslatedUserId(request);
                }

                if (_options.Features.ForwardNexusUserAuthorization.Enabled)
                {
                    ForwardNexusUserAuthorization(request);
                }

                if (_options.Features.ForwardNexusTestContext.Enabled)
                {
                    ForwardNexusTestContext(request);
                }
            }

            async Task<HttpResponseMessage> SendRequestAsync()
            {
                if (_options.Features.CustomSendDelegate.Enabled)
                {
                    var sendAsyncDelegate = _options.Features.CustomSendDelegate.SendAsyncDelegate;
                    FulcrumAssert.IsNotNull(sendAsyncDelegate, CodeLocation.AsString());
                    response = await sendAsyncDelegate(request, cancellationToken);
                }
                else
                {
                    response = await base.SendAsync(request, cancellationToken);
                }

                return response;
            }

            async Task HandleResponseAsync()
            {
                if (_options.Features.LogRequestAndResponse.Enabled)
                {
                    await LogRequestResponseAsync(request, response, timer.Elapsed, cancellationToken);
                }

                if (_options.Features.ThrowFulcrumExceptionOnFail.Enabled)
                {
                    await ThrowFulcrumExceptionBasedOnResponse(request, response, cancellationToken);
                }
            }

            void HandleExceptionsAndMaybeThrowNewException(Exception exception)
            {
                var actualException = exception;
                try
                {
                    if (_options.Features.ThrowFulcrumExceptionOnFail.Enabled)
                    {
                        ThrowFulcrumExceptionBasedOnException(request, exception);
                    }
                }
                catch (Exception e)
                {
                    actualException = e;
                    throw;
                }
                finally
                {
                    if (_options.Features.LogRequestAndResponse.Enabled)
                    {
                        LogRequestException(request, actualException, timer.Elapsed);
                    }
                }
            }
        }

        private static void ThrowFulcrumExceptionBasedOnException(HttpRequestMessage request, Exception exception)
        {
            var requestDescription = $"OUT request {request.ToLogString()}";
            switch (exception)
            {
                case FulcrumException:
                case RequestAcceptedException:
                case RequestPostponedException:
                    break;
                case TaskCanceledException e:
                    {
                        var message = $"{requestDescription} was cancelled.";
                        Log.LogWarning(message, e);
                        throw new FulcrumTryAgainException(message, e);
                    }
                case HttpRequestException or JsonReaderException:
                    {
                        var message = $"{requestDescription} failed: {exception.GetType().Name} {exception.Message}.";
                        Log.LogWarning(message, exception);
                        throw new FulcrumAssertionFailedException(message, exception);
                    }
                default:
                    {
                        // If we end up here, we probably need to add another catch statement for that exception type.
                        var message =
                            $"{requestDescription} failed with an exception of type {exception.GetType().FullName}." +
                            $" Please report that feature {nameof(NexusLinkHandlerOptions.Features.ThrowFulcrumExceptionOnFail)}" +
                            $" of handler {nameof(NexusLinkHandler)} if you think it should convert this exception type explicitly.";
                        Log.LogError(message, exception);
                        throw new FulcrumAssertionFailedException(message, exception);
                    }
            }
        }

        private static async Task ThrowFulcrumExceptionBasedOnResponse(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var requestDescription = $"OUT request-response {await request.ToLogStringAsync(response, default, cancellationToken)}";
            if (response.StatusCode == HttpStatusCode.Accepted && response.Content != null)
            {
                await response.Content.LoadIntoBufferAsync();
                var content = await response.Content.ReadAsStringAsync();
                var acceptInfo = JsonHelper.SafeDeserializeObject<RequestAcceptedContent>(content);
                if (acceptInfo?.RequestId != null)
                {
                    var requestAcceptedException = new RequestAcceptedException(acceptInfo.RequestId)
                    {
                        PollingUrl = acceptInfo.PollingUrl,
                        RegisterCallbackUrl = acceptInfo.RegisterCallbackUrl
                    };
                    Log.LogInformation($"{requestDescription} was converted to (and threw) the exception {requestAcceptedException.GetType().Name}");
                    throw requestAcceptedException;
                }

                var postponeInfo = JsonHelper.SafeDeserializeObject<RequestPostponedContent>(content);
                if (postponeInfo?.WaitingForRequestIds == null) return;
                var timeSpan = postponeInfo.TryAgainAfterMinimumSeconds.HasValue
                    ? TimeSpan.FromSeconds(postponeInfo.TryAgainAfterMinimumSeconds.Value)
                    : (TimeSpan?)null;
                var requestPostponedException = new RequestPostponedException(postponeInfo.WaitingForRequestIds)
                {
                    TryAgain = postponeInfo.TryAgain,
                    TryAgainAfterMinimumTimeSpan = timeSpan,
                    ReentryAuthentication = postponeInfo.ReentryAuthentication
                };
                Log.LogInformation($"{requestDescription} was converted to (and threw) the exception {requestPostponedException.GetType().Name}");
                throw requestPostponedException;
            }

            var fulcrumException = await ExceptionConverter.ToFulcrumExceptionAsync(response, cancellationToken);
            if (fulcrumException == null) return;
            var severityLevel = (int)response.StatusCode >= 500 ? LogSeverityLevel.Error : LogSeverityLevel.Warning;
            Log.LogOnLevel(severityLevel, $"{requestDescription} was converted to (and threw) the exception {fulcrumException.GetType().Name}: {fulcrumException.TechnicalMessage}", fulcrumException);
            throw fulcrumException;
        }

        private static void ForwardNexusTestContext(HttpRequestMessage request)
        {
            if (request.Headers.TryGetValues(Constants.NexusTestContextHeaderName, out _)) return;
            var headerValue = FulcrumApplication.Context.NexusTestContext;
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                request.Headers.Add(Constants.NexusTestContextHeaderName, headerValue);
            }
        }

        private async Task LogRequestResponseAsync(HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsedTime, CancellationToken cancellationToken)
        {
            if (request == null) return;
            LogSeverityLevel level;
            if (response.IsSuccessStatusCode) level = LogSeverityLevel.Information;
            else if ((int)response.StatusCode >= 500) level = LogSeverityLevel.Warning;
            else if ((int)response.StatusCode >= 400) level = LogSeverityLevel.Error;
            else level = LogSeverityLevel.Warning;
            Log.LogOnLevel(level, $"OUTBOUND request-response {await request.ToLogStringAsync(response, elapsedTime, cancellationToken)}");
        }

        private void LogRequestException(HttpRequestMessage request, Exception exception, TimeSpan elapsedTime)
        {
            Log.LogError($"OUTBOUND request-exception {request.ToLogString(elapsedTime)} | {exception.Message}", exception);
        }

        private void ForwardNexusUserAuthorization(HttpRequestMessage request)
        {
            var options = _options.Features.ForwardNexusUserAuthorization;
            var userAuthorization = options.ContextValueProvider.GetValue<string>(Constants.NexusUserAuthorizationKeyName);

            if (string.IsNullOrWhiteSpace(userAuthorization)) return;
            if (!request.Headers.TryGetValues(Constants.NexusUserAuthorizationHeaderName, out _))
            {
                request.Headers.Add(Constants.NexusUserAuthorizationHeaderName, userAuthorization);
            }
        }

        private void ForwardNexusTranslatedUserId(HttpRequestMessage request)
        {
            var options = _options.Features.ForwardNexusTranslatedUserId;
            var translatedUserId = options.ContextValueProvider.GetValue<string>(Constants.TranslatedUserIdKey);

            if (string.IsNullOrWhiteSpace(translatedUserId)) return;
            if (!request.Headers.TryGetValues(Constants.NexusTranslatedUserIdHeaderName, out _))
            {
                request.Headers.Add(Constants.NexusTranslatedUserIdHeaderName, translatedUserId);
            }
        }

        private static void ForwardCorrelationId(HttpRequestMessage request)
        {
            if (string.IsNullOrWhiteSpace(FulcrumApplication.Context.CorrelationId))
            {
                if (FulcrumApplication.Context.CallingClientName == null) return;
                // We should have a gotten a correlation id from the calling client.
                var logLevel = FulcrumApplication.IsInProductionOrProductionSimulation
                    ? LogSeverityLevel.Verbose
                    : LogSeverityLevel.Warning;
                Log.LogOnLevel(logLevel,
                    $"We have a calling client ({FulcrumApplication.Context.CallingClientName}), but we are missing a correlation id for an outbound request ({request.ToLogString()}).");
            }
            else
            {
                if (request.Headers.TryGetValues(Constants.FulcrumCorrelationIdHeaderName, out _)) return;
                request.Headers.Add(Constants.FulcrumCorrelationIdHeaderName,
                    FulcrumApplication.Context.CorrelationId);
            }
        }
    }
}
