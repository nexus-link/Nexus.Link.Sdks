using System.Net;
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

        /// <inheritdoc />
        public async Task SuccessAsync(string id, ActivityInstanceSuccessResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(result, nameof(result));
            InternalContract.RequireValidated(result, nameof(result));

            var relativeUrl = $"{WebUtility.UrlEncode(id)}/Success";
            await PostNoResponseContentAsync(relativeUrl, result, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task FailedAsync(string id, ActivityInstanceFailedResult result, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(result, nameof(result));
            InternalContract.RequireValidated(result, nameof(result));

            var relativeUrl = $"{WebUtility.UrlEncode(id)}/Failed";
            await PostNoResponseContentAsync(relativeUrl, result, cancellationToken: cancellationToken);
        }
    }
}