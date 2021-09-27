using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncManager.Sdk.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Serialization;
using WorkflowEngine.Sdk.Inbound.RespondAsync.Logic;

namespace WorkflowEngine.Sdk.Inbound
{
    public class ResponseHandler : ResponseHandlerBase
    {
        public IAsyncManagementCapabilityForClient AsyncManagementCapability { get; }

        /// <inheritdoc />
        public ResponseHandler(IAsyncManagementCapabilityForClient asyncManagementCapability, string urlFormat) : base(urlFormat)
        {
            AsyncManagementCapability = asyncManagementCapability;
        }

        public override async Task<ResponseData> GetResponseAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            var response = await AsyncManagementCapability.Response.GetResponseAsync(requestId.ToString(), cancellationToken);
            return response;
        }

        /// <inheritdoc />
        public override Task AddResponse(RequestData requestData, ResponseData responseData)
        {
            if (responseData != null)
            {
                var success = RespondAsyncFilterSupport.TryGetExecutionId(requestData.Headers, out var executionId);
                FulcrumAssert.IsTrue(success, CodeLocation.AsString());
                // Serialize the response and make it available to the caller
                AsyncManagementCapability.Response.CreateResponse(executionId.ToString(), responseData.BodyAsString);
            }
            return Task.CompletedTask;
        }
    }
}