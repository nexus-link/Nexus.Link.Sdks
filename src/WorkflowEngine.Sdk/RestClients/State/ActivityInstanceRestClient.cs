using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.State
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

            return PostAsync<ActivityInstance, ActivityInstanceUnique>("FindUnique", findUnique, cancellationToken: cancellationToken);
        }
    }
}