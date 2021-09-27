#if NETCOREAPP
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Serialization;

namespace WorkflowEngine.Sdk.Inbound.RespondAsync.Logic
{
    public class ResponseHandlerInMemory : ResponseHandlerBase
    {
        private static readonly ConcurrentDictionary<Guid, ResponseData> ResponsesByRequestId = new ConcurrentDictionary<Guid, ResponseData>();

        /// <inheritdoc />
        public ResponseHandlerInMemory(string urlFormat) : base(urlFormat)
        {
        }

        public override Task<ResponseData> GetResponseAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            var response = ResponsesByRequestId.TryGetValue(requestId, out var responseData) ? responseData : AcceptedResponse(requestId);
            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public override Task AddResponse(RequestData requestData, ResponseData responseData)
        {
            // Serialize the response and make it available to the caller
            FulcrumAssert.IsNotNull(requestData.Id);
            ResponsesByRequestId.TryAdd(requestData.Id!.Value, responseData);
            return Task.CompletedTask;
        }
    }
}
#endif