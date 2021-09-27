using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncManager.Sdk.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Libraries.Web.Serialization;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;

namespace WorkflowEngine.Sdk.Outbound
{
    public class CallAsyncManagerForAsynchronousRequests : DelegatingHandler
    {
        private readonly IAsyncManagementCapabilityForClient _asyncManagementCapability;

        public CallAsyncManagerForAsynchronousRequests(IAsyncManagementCapabilityForClient asyncManagementCapability)
        {
            _asyncManagementCapability = asyncManagementCapability;
        }

        /// <summary>
        /// Adds a Fulcrum CorrelationId to the requests before sending it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="PostponeException"></exception>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!AsyncWorkflowStatic.Context.ExecutionIsAsynchronous)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            // Send the request to AM
            var requestData = await new RequestData().FromAsync(request, cancellationToken);
            FulcrumAssert.IsTrue(!requestData.Headers.ContainsKey(Constants.ExecutionIdHeaderName), CodeLocation.AsString());
            var executionId = AsyncWorkflowStatic.Context.AsyncExecutionContext?.ExecutionId;
            FulcrumAssert.IsNotNull(executionId, CodeLocation.AsString());
            var requestId = await _asyncManagementCapability.Request.CreateAsyncRequestAsync(executionId!.Value.ToString(), requestData, cancellationToken);
            throw new PostponeException(requestId);
        }
    }
}