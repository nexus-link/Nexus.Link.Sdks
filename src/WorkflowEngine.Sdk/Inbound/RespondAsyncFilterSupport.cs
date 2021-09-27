using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Queue.Logic;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Libraries.Web.Serialization;
using Nexus.Link.WorkflowEngine.Sdk.Inbound.RespondAsync.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Inbound
{
    public class RespondAsyncFilterSupport : DefaultRespondAsyncFilterSupport
    {
        public IAsyncRequestMgmtCapability AsyncManager { get; }

        public RespondAsyncFilterSupport(IAsyncRequestMgmtCapability asyncManager, HttpClient httpClient, string asyncManagerUrl)
        : base(
            new ChannelQueue<RequestData>(100),
            new RequestExecutor(asyncManager, httpClient),
            new ResponseHandler(asyncManager, asyncManagerUrl + @"/requests/{0}/responses"))
        {
            AsyncManager = asyncManager;
        }

        /// <inheritdoc />
        protected override async Task EnqueueAsync(RequestData requestData, CancellationToken cancellationToken)
        {
            var requestId = await AddExecutionIdHeader(requestData, cancellationToken);
            requestData.Id = requestId;
            await base.EnqueueAsync(requestData, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The requestId</returns>
        /// <remarks>Side effect: Adds a execution id header if missing.</remarks>
        private async Task<Guid?> AddExecutionIdHeader(RequestData requestData, CancellationToken cancellationToken)
        {
            throw new FulcrumNotImplementedException(nameof(AddExecutionIdHeader));

            //if (TryGetExecutionId(requestData.Headers, out var executionId)) return null;
            //var execution = await AsyncManager.Request.CreateAsyncExecutionAsync(requestData, cancellationToken);
            //requestData.Headers.Add(Constants.ExecutionIdHeaderName, execution.Id.ToString());
            //return execution.RequestId;
        }

        public static bool TryGetExecutionId(Dictionary<string, StringValues> headers, out Guid executionId)
        {
            executionId = Guid.Empty;
            headers.TryGetValue(Constants.ExecutionIdHeaderName, out var executionIdString);
            if (string.IsNullOrWhiteSpace(executionIdString)) return false;

            var success = Guid.TryParse(executionIdString, out executionId);
            FulcrumAssert.IsTrue(success, CodeLocation.AsString());
            return true;
        }
    }
}
