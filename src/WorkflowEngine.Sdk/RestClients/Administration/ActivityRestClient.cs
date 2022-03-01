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
    /// <inheritdoc cref="IActivityService" />
    public class ActivityRestClient : CrudRestClient<Activity, string>, IActivityService
    {
        /// <summary>
        /// Controller
        /// </summary>
        public ActivityRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Workflows"))
        {
        }

        /// <inheritdoc />
        public Task SuccessAsync(string id, ActivitySuccessResult result,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(result, nameof(result));
            var relativeUrl = $"{WebUtility.UrlEncode(id)}/Success";
            return PostNoResponseContentAsync(relativeUrl, result, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task FailedAsync(string id, ActivityFailedResult result, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(result, nameof(result));
            var relativeUrl = $"{WebUtility.UrlEncode(id)}/Failed";
            return PostNoResponseContentAsync(relativeUrl, result, null, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RetryAsync(string activityInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(activityInstanceId, nameof(activityInstanceId));

            var relativeUrl = $"{WebUtility.UrlEncode(activityInstanceId)}/Retry";
            await PostNoResponseContentAsync(relativeUrl, cancellationToken: cancellationToken);
        }
    }
}
