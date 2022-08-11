using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{
    /// <inheritdoc cref="IWorkflowService" />
    public class WorkflowRestClient : CrudRestClient<Workflow, string>, IWorkflowService
    {
        /// <summary>
        /// Controller
        /// </summary>
        public WorkflowRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Instances"))
        {
        }

        /// <inheritdoc />
        public async Task CancelAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(workflowInstanceId, nameof(workflowInstanceId));

            var relativeUrl = $"{WebUtility.UrlEncode(workflowInstanceId)}/Cancel";
            await PostNoResponseContentAsync(relativeUrl, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<Workflow>> SearchAsync(WorkflowSearchDetails searchDetails, int offset = 0, int limit = 50, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(searchDetails, nameof(searchDetails));
            InternalContract.RequireValidated(searchDetails, nameof(searchDetails));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));

            return await PostAsync<PageEnvelope<Workflow>, WorkflowSearchDetails>("", searchDetails, cancellationToken: cancellationToken);
        }
    }
}
