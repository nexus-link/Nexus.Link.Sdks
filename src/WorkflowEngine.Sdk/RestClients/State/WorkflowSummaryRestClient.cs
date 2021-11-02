using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.State
{

    public class WorkflowSummaryRestClient : CrudRestClient<WorkflowSummary, string>, IWorkflowSummaryService
    {
        public WorkflowSummaryRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("WorkflowSummaries"))
        {
        }

        /// <inheritdoc />
        public Task<WorkflowSummary> GetSummaryAsync(string formId, int majorVersion, string instanceId,
            CancellationToken cancellationToken = default)
        {
            return GetAsync<WorkflowSummary>(
                $"Forms/{formId}/Versions/{majorVersion}/Instances/{instanceId}",
                null, cancellationToken);
        }

        /// <inheritdoc />
        public Task<WorkflowSummary> GetSummaryAsync(string instanceId, CancellationToken cancellationToken = default)
        {
            return GetAsync<WorkflowSummary>($"Instances/{instanceId}", null, cancellationToken);
        }
    }
}