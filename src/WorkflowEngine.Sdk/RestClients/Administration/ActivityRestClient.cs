using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{

    public class ActivityRestClient : CrudRestClient<Activity, string>, IActivityService
    {
        public ActivityRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Workflows"))
        {
        }

        /// <inheritdoc />
        public Task SuccessAsync(string activityInstanceId, ActivitySuccessResult result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));
            InternalContract.RequireNotNull(result, nameof(result));
            var relativeUrl = $"{WebUtility.UrlEncode(activityInstanceId)}/Success";
            return PostNoResponseContentAsync(relativeUrl, result, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task FailedAsync(string activityInstanceId, ActivityFailedResult result, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));
            InternalContract.RequireNotNull(result, nameof(result));
            var relativeUrl = $"{WebUtility.UrlEncode(activityInstanceId)}/Failed";
            return PostNoResponseContentAsync(relativeUrl, result, null, cancellationToken);
        }
    }
}
