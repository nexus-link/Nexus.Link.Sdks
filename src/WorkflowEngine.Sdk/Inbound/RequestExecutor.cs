using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Libraries.Web.Serialization;
using Nexus.Link.WorkflowEngine.Sdk.Inbound.RespondAsync.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Inbound
{
    public class RequestExecutor : RequestExecutorBase
    {
        public IAsyncRequestMgmtCapability AsyncMgmtCapability { get; }

        public RequestExecutor(IAsyncRequestMgmtCapability asyncManagementCapability, HttpClient httpClient) : base(httpClient)
        {
            // TODO: Change RequestExecutorBase to accept the HttpClient in the constructor. 
            HttpClient = httpClient;
            AsyncMgmtCapability = asyncManagementCapability;
        }

        /// <inheritdoc />
        public override async Task<ResponseData> ExecuteRequestAsync(RequestData requestData, CancellationToken cancellationToken = new CancellationToken())
        {
            var result =  await base.ExecuteRequestAsync(requestData, cancellationToken);
            return result;
        }
    }
}