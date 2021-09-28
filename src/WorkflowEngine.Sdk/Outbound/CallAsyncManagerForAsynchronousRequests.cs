using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
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
        private readonly IAsyncRequestClient _asyncRequestClient;

        public CallAsyncManagerForAsynchronousRequests(IAsyncRequestClient asyncRequestClient)
        {
            _asyncRequestClient = asyncRequestClient;
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
            var requestId = await _asyncRequestClient
                .CreateRequest(request.Method, request.RequestUri.AbsoluteUri, 0.5)
                .AddHeaders(request.Headers)
                .SendAsync(cancellationToken);
            // TODO: Set callback
            throw new PostponeException(requestId);
        }
    }
}