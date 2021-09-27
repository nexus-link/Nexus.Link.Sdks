using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.Serialization;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class AsyncContextRestClient : RestClient, IAsyncContextService
    {

        public AsyncContextRestClient(IHttpSender httpSender)
        :base(httpSender.CreateHttpSender("contexts"))
        {
        }

        /// <inheritdoc />
        public Task<AsyncExecutionContext> GetExecutionContextAsync(string executionId, RequestData requestData,
            CancellationToken cancellationToken = default)
        {
            return PostAsync<AsyncExecutionContext, RequestData>($"executions/{executionId}/contexts",
                requestData, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task AddSubRequestAsync(string executionId, string identifier, SubRequest subRequest,
            CancellationToken cancellationToken = default)
        {
            return PostNoResponseContentAsync($"executions/{executionId}/sub-requests/{identifier}",
                subRequest, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task<string> GetStatusAsStringAsync(string requestId, CancellationToken cancellationToken = default)
        {
            return GetAsync<string>($"requests/{requestId}/statusAsString", null, cancellationToken);
        }

        /// <inheritdoc />
        public Task<JObject> GetStatusAsJsonAsync(string requestId, CancellationToken cancellationToken = default)
        {
            return GetAsync<JObject>($"requests/{requestId}/statusAsJson", null, cancellationToken);
        }
    }
}