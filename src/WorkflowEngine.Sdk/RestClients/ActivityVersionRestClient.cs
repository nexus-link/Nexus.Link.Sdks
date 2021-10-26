using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class ActivityVersionRestClient : CrudRestClient<ActivityVersionCreate, ActivityVersion, string>, IActivityVersionService
    {
        public ActivityVersionRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("ActivityVersions"))
        {
        }

        /// <inheritdoc />
        public Task<ActivityVersion> FindUniqueAsync(string workflowVersionId, string activityFormId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            InternalContract.RequireNotNullOrWhiteSpace(activityFormId, nameof(activityFormId));

            var relativeUrl = $"?workflowVersionId={WebUtility.UrlEncode(workflowVersionId)}&activityFormId={WebUtility.UrlEncode(activityFormId)}";
            return GetAsync<ActivityVersion>(relativeUrl, cancellationToken: cancellationToken);
        }
    }
}