using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{
    /// <inheritdoc cref="IActivityService" />
    public class ActivityRestClient : CrudRestClient<Activity, string>, IActivityService
    {
        /// <summary>
        /// Controller
        /// </summary>
        public ActivityRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Activities"))
        {
        }

        /// <inheritdoc />
        public Task SuccessAsync(string id, ActivitySuccessResult result,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            InternalContract.RequireNotNull(result, nameof(result));
            var relativeUrl = $"{WebUtility.UrlEncode(id)}/Success";
            return PostNoResponseContentAsync(relativeUrl, result, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task FailedAsync(string id, ActivityFailedResult result, CancellationToken cancellationToken = default)
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
