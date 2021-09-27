using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;

namespace WorkflowEngine.Sdk.RestClients
{
    public class ActivityVersionRestClient : CrudManyToOneRestClient2<ActivityVersionCreate, ActivityVersion, string>, IActivityVersionService
    {
        public ActivityVersionRestClient(IHttpSender httpSender) : base(httpSender, "workflow-versions", "activity-versions")
        {
        }

        /// <inheritdoc />
        public Task<ActivityVersion> FindUniqueByWorkflowVersionActivityAsync(string workflowVersionId, string activityId,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            InternalContract.RequireNotNullOrWhiteSpace(activityId, nameof(activityId));
            return GetAsync<ActivityVersion>($"workflow-versions/{workflowVersionId}/activities/{activityId}/activity-version", cancellationToken: cancellationToken);
        }
    }
}