using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Libraries.Web.Serialization;
using Nexus.Link.WorkflowEngine.Sdk.Extensions;

namespace Nexus.Link.WorkflowEngine.Sdk.Outbound
{
    public class CallAsyncManagerForAsynchronousRequests : DelegatingHandler
    {
        private readonly IAsyncRequestMgmtCapability _asyncManagementCapability;

        public CallAsyncManagerForAsynchronousRequests(IAsyncRequestMgmtCapability asyncManagementCapability)
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

            // TODO: Use new version
            //var requestId = await _asyncManagementCapability.Request.CreateAsyncRequestAsync(executionId!.Value.ToString(), requestData, cancellationToken);
            var item = new HttpRequestCreate().From(requestData);
            var requestId = await _asyncManagementCapability.Request.CreateAsync(item, cancellationToken);
            // TODO: PostponeException should take string
            throw new PostponeException(MapperHelper.MapToType<Guid, string>(requestId));
        }
    }
}