using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class ActivityVersionRestClient : CrudManyToOneRestClient2<ActivityVersionCreate, ActivityVersion, string>, IActivityVersionService
    {
        public ActivityVersionRestClient(IHttpSender httpSender) : base(httpSender, "workflow-versions", "activity-versions")
        {
        }

        /// <inheritdoc />
        public Task<ActivityVersion> FindUniqueAsync(string workflowVersionId, string activityId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            InternalContract.RequireNotNullOrWhiteSpace(activityId, nameof(activityId));
            return GetAsync<ActivityVersion>($"workflow-versions/{workflowVersionId}/activities/{activityId}/activity-version", cancellationToken: cancellationToken);
        }
    }
}