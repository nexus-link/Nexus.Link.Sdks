﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Serialization;
using Nexus.Link.WorkflowEngine.Sdk.Inbound.RespondAsync.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Extensions;

namespace Nexus.Link.WorkflowEngine.Sdk.Inbound
{
    public class ResponseHandler : ResponseHandlerBase
    {
        public IAsyncRequestClient AsyncRequestClient { get; }

        /// <inheritdoc />
        public ResponseHandler(IAsyncRequestClient asyncRequestClient, string urlFormat) : base(urlFormat)
        {
            AsyncRequestClient = asyncRequestClient;
        }

        public override async Task<ResponseData> GetResponseAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            throw new FulcrumNotImplementedException(nameof(GetResponseAsync));
            //var asyncMgmtResponse= await AsyncRequestClient.ReadResponseAsync(requestId.ToString(), cancellationToken);
            //var response = new ResponseData().From(asyncMgmtResponse);
            //return response;
        }

        /// <inheritdoc />
        public override Task AddResponse(RequestData requestData, ResponseData responseData)
        {
            throw new FulcrumNotImplementedException(nameof(AddResponse));
            //if (responseData != null)
            //{
            //    var success = RespondAsyncFilterSupport.TryGetExecutionId(requestData.Headers, out var executionId);
            //    FulcrumAssert.IsTrue(success, CodeLocation.AsString());
            //    // Serialize the response and make it available to the caller
            //    AsyncMgmtCapability.Response.CreateResponse(executionId.ToString(), responseData.BodyAsString);
            //}
            //return Task.CompletedTask;
        }
    }
}