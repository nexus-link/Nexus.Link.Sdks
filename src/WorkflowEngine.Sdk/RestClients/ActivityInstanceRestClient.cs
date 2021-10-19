using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class ActivityInstanceRestClient : CrudRestClient<ActivityInstanceCreate, ActivityInstance, string>, IActivityInstanceService
    {
        public ActivityInstanceRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("ActivityInstances"))
        {
        }

        /// <inheritdoc />
        public Task<ActivityInstance> FindUniqueAsync(ActivityInstanceUnique findUnique, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(findUnique, nameof(findUnique));

            return PostAsync<ActivityInstance, ActivityInstanceUnique>("ActivityInstances/FindUnique", findUnique, cancellationToken: cancellationToken);
        }
    }
}