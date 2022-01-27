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
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Outbound
{
    public class CallAsyncManagerForAsynchronousRequests : DelegatingHandler
    {
        private readonly IAsyncRequestMgmtCapability _asyncRequestMgmtCapability;

        public CallAsyncManagerForAsynchronousRequests(IAsyncRequestMgmtCapability asyncRequestMgmtCapability)
        {
            _asyncRequestMgmtCapability = asyncRequestMgmtCapability;
        }

        /// <summary>
        /// Adds a Fulcrum CorrelationId to the requests before sending it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="RequestPostponedException"></exception>
        /// <returns></returns>
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
                if (response != null) return response;
                throw new RequestPostponedException(activity.Instance.AsyncRequestId);
            }

            // Send the request to AM
            var asyncRequest = await new HttpRequestCreate().FromAsync(request, activity.Options.AsyncRequestPriority, cancellationToken);
            var requestId = await _asyncRequestMgmtCapability.Request.CreateAsync(asyncRequest, cancellationToken);

            // Remember the request id and postpone the activity.
            throw new RequestPostponedException(requestId);
        }

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