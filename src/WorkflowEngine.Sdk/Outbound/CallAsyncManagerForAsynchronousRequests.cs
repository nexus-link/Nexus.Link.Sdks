using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk.Extensions;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Outbound
{
    /// <summary>
    /// If the request is in an asynchronous context, the request will be sent over an <see cref="IAsyncRequestMgmtCapability"/>.
    /// </summary>
    public class CallAsyncManagerForAsynchronousRequests : DelegatingHandler
    {
        private readonly IAsyncRequestMgmtCapability _asyncRequestMgmtCapability;

        /// <summary>
        /// Constructor
        /// </summary>
        public CallAsyncManagerForAsynchronousRequests(IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        {
            _asyncRequestMgmtCapability = asyncRequestMgmtCapability;
        }

        /// <summary>
        /// If the request is in an asynchronous context, the request will be sent over an <see cref="IAsyncRequestMgmtCapability"/>.
        /// </summary>
        /// <exception cref="RequestPostponedException">
        /// Thrown after the request has been sent for asynchronous handling and when a response is not available.
        /// </exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!WorkflowStatic.Context.ExecutionIsAsynchronous)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var activity = WorkflowStatic.Context.LatestActivity;
            FulcrumAssert.IsNotNull(activity, CodeLocation.AsString());
            FulcrumAssert.IsNotNull(activity.Instance, CodeLocation.AsString());

            if (activity.Instance.AsyncRequestId != null)
            {
                HttpResponseMessage response;
                try
                {
                    response = await TryGetResponseAsync(request, activity, cancellationToken);
                }
                catch (RequestPostponedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var timeSpan = ex is FulcrumException fe
                        ? TimeSpan.FromSeconds(fe.RecommendedWaitTimeInSeconds)
                        : TimeSpan.FromSeconds(60);
                    throw new ActivityPostponedException(timeSpan);
                }

                if (response != null)
                {
                    await activity.LogInformationAsync($"Activity received a response"+
                                       $" on request {request.ToLogString()}", activity, cancellationToken);
                    return response;
                }
                await activity.LogVerboseAsync($"Activity polled for a response" +
                                                   $" to request {request.ToLogString()}", activity, cancellationToken);
                throw new ActivityWaitsForRequestException(activity.Instance.AsyncRequestId);
            }

            // Send the request to AM
            var asyncRequest = await new HttpRequestCreate().FromAsync(request, activity.Options.AsyncRequestPriority, cancellationToken);
            asyncRequest.Metadata.WaitingRequestId = WorkflowStatic.Context?.WorkflowInstanceId;
            var requestId = await _asyncRequestMgmtCapability.Request.CreateAsync(asyncRequest, cancellationToken);
            await activity.LogInformationAsync($"Activity sent the request {request.ToLogString()} for asynchronous execution.", activity, cancellationToken);

            // Remember the request id and postpone the activity.
            throw new ActivityWaitsForRequestException(requestId);
        }


        private async Task<HttpResponseMessage> TryGetResponseAsync(HttpRequestMessage request, IInternalActivity activity, CancellationToken cancellationToken)
        {
            InternalContract.Require(!activity.Instance.HasCompleted, "The activity instance must not be completed.");


            if (activity.ActivityInformation.Workflow.HttpAsyncResponses == null)
            {
                await activity.ActivityInformation.Workflow.ReadResponsesSemaphore
                    .ExecuteAsync(ct => MaybeReadResponsesAsync(activity, 100, TimeSpan.FromSeconds(5), ct), cancellationToken);
                FulcrumAssert.IsNotNull(activity.ActivityInformation.Workflow.HttpAsyncResponses, CodeLocation.AsString());
            }

            if (!activity.ActivityInformation.Workflow.HttpAsyncResponses!.TryGetValue(
                    activity.Instance.AsyncRequestId, out var response)) return null;
            if (!response.Metadata.RequestHasCompleted) return null;

            if (response.HttpStatus == null)
            {
                throw new ActivityFailedException(
                    ActivityExceptionCategoryEnum.TechnicalError,
                    $"A remote method returned an exception with the name {response.Exception.Name} and message: {response.Exception.Message}",
                    $"A remote method failed with the following message: {response.Exception.Message}");
            }

            var responseMessage = response.ToHttpResponseMessage(request);
            return responseMessage;
        }

        private async Task MaybeReadResponsesAsync(IInternalActivity activity, int? limit, TimeSpan? timeLimit, CancellationToken cancellationToken)
        {
            if (activity.ActivityInformation.Workflow.HttpAsyncResponses != null) return;

            var waitingRequestId = activity.WorkflowInstanceId;
            var responses = await _asyncRequestMgmtCapability.RequestResponse.ReadWaitingResponsesAsync(waitingRequestId, limit, timeLimit?.TotalSeconds, cancellationToken);
            activity.ActivityInformation.Workflow.HttpAsyncResponses = responses
                .ToDictionary(r => r.Id, r => r);
        }
    }
}