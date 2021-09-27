using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncManager.Sdk.Abstract;
using Nexus.Link.Libraries.Web.Serialization;
using WorkflowEngine.Sdk.Inbound.RespondAsync.Logic;

namespace WorkflowEngine.Sdk.Inbound
{
    public class RequestExecutor : RequestExecutorBase
    {
        public IAsyncManagementCapabilityForClient AsyncManagementCapability { get; }

        public RequestExecutor(IAsyncManagementCapabilityForClient asyncManagementCapability, HttpClient httpClient) : base(httpClient)
        {
            // TODO: Change RequestExecutorBase to accept the HttpClient in the constructor. 
            HttpClient = httpClient;
            AsyncManagementCapability = asyncManagementCapability;
        }

        /// <inheritdoc />
        public override async Task<ResponseData> ExecuteRequestAsync(RequestData requestData, CancellationToken cancellationToken = new CancellationToken())
        {
            var result =  await base.ExecuteRequestAsync(requestData, cancellationToken);
            return result;
        }
    }
}