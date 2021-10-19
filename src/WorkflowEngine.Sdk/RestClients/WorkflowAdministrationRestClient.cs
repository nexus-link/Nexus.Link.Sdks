using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{

    public class WorkflowAdministrationRestClient : CrudRestClient<Workflow, string>, IWorkflowAdministrationService
    {
        public WorkflowAdministrationRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Workflows"))
        {
        }

        /// <inheritdoc />
        public async Task CancelWorkflowAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(workflowInstanceId, nameof(workflowInstanceId));

            var relativeUrl = $"Workflows/{WebUtility.UrlEncode(workflowInstanceId)}/Cancel";
            await PostNoResponseContentAsync(relativeUrl, cancellationToken: cancellationToken);
        }
    }
}
