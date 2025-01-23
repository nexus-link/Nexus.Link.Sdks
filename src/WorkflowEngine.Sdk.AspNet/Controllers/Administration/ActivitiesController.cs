using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Administration
{
    /// <inheritdoc cref="IWorkflowService" />
    public abstract class ActivitiesController : ControllerBase, IActivityService
    {
        private readonly IWorkflowMgmtCapability _capability;

        /// <summary>
        /// Controller
        /// </summary>
        protected ActivitiesController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }


        /// <inheritdoc />
        [HttpPost("Activities/{activityInstanceId}/Success")]
        public Task SuccessAsync(string activityInstanceId, ActivitySuccessResult result,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));

            return _capability.Activity.SuccessAsync(activityInstanceId, result, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost("Activities/{activityInstanceId}/Failed")]
        public Task FailedAsync(string activityInstanceId, ActivityFailedResult result, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));

            return _capability.Activity.FailedAsync(activityInstanceId, result, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost("Activities/{activityInstanceId}/Retry")]
        public async Task RetryAsync(string activityInstanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));

            await _capability.Activity.RetryAsync(activityInstanceId, cancellationToken);
        }
    }
}
