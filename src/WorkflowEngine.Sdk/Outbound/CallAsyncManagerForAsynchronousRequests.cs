using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Exceptions;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support;
using Nexus.Link.Libraries.Web.Error.Logic;

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
            // TODO: Set callback <L
            throw new RequestAcceptedException(null, requestId);
        }
    }
}