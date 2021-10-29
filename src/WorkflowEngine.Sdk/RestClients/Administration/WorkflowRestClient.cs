using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Administration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{

    public class WorkflowRestClient : CrudRestClient<Workflow, string>, IWorkflowService
    {
        public WorkflowRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Workflows"))
        {
        }

        /// <inheritdoc />
        public async Task CancelWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(workflowInstanceId, nameof(workflowInstanceId));

            var relativeUrl = $"{WebUtility.UrlEncode(workflowInstanceId)}/Cancel";
            await PostNoResponseContentAsync(relativeUrl, cancellationToken: cancellationToken);
        }

        public async Task RetryActivityAsync(string activityInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(activityInstanceId, nameof(activityInstanceId));

            var relativeUrl = $"{WebUtility.UrlEncode(activityInstanceId)}/Retry";
            await PostNoResponseContentAsync(relativeUrl, cancellationToken: cancellationToken);
        }
    }
}
