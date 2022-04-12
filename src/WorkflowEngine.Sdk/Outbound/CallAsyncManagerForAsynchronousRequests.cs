using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk.Extensions;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Log = Nexus.Link.Libraries.Core.Logging.Log;

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
                var response = await TryGetResponseAsync(request, activity, cancellationToken);
                if (response != null)
                {
                    Log.LogInformation($"Activity {ToLogString(activity)} received a response"+
                                       $" on request {request.ToLogString()}");
                    return response;
                }
                Log.LogVerbose($"Activity {ToLogString(activity)} polled for a response" +
                               $" to request {request.ToLogString()}");
                throw new RequestPostponedException(activity.Instance.AsyncRequestId);
            }

            // Send the request to AM
            var asyncRequest = await new HttpRequestCreate().FromAsync(request, activity.Options.AsyncRequestPriority, cancellationToken);
            var requestId = await _asyncRequestMgmtCapability.Request.CreateAsync(asyncRequest, cancellationToken);
            Log.LogInformation($"Activity {ToLogString(activity)} sent the request {request.ToLogString()} for asynchronous execution.");

            // Remember the request id and postpone the activity.
            throw new RequestPostponedException(requestId);
        }

        private string ToLogString(Activity activity) =>
            $"{activity} in workflow instance {WorkflowStatic.Context.WorkflowInstanceId}";

        private async Task<HttpResponseMessage> TryGetResponseAsync(HttpRequestMessage request, Activity activity, CancellationToken cancellationToken)
        {
            InternalContract.Require(!activity.Instance.HasCompleted, "The activity instance must not be completed.");
            var asyncResponse = await _asyncRequestMgmtCapability.RequestResponse.ReadResponseAsync(activity.Instance.AsyncRequestId,
                cancellationToken);
            if (asyncResponse == null || !asyncResponse.Metadata.RequestHasCompleted) return null;

            if (asyncResponse.HttpStatus == null)
            {
                throw new ActivityException(
                    ActivityExceptionCategoryEnum.TechnicalError,
                    $"A remote method returned an exception with the name {asyncResponse.Exception.Name} and message: {asyncResponse.Exception.Message}",
                    $"A remote method failed with the following message: {asyncResponse.Exception.Message}");
            }

            var response = asyncResponse.ToHttpResponseMessage(request);
            return response;
        }
    }
}