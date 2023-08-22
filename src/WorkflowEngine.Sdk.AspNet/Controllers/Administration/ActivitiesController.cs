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
        [HttpPost("Activities/{id}/Success")]
        public Task SuccessAsync(string id, ActivitySuccessResult result,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return _capability.Activity.SuccessAsync(id, result, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost("Activities/{id}/Failed")]
        public Task FailedAsync(string id, ActivityFailedResult result, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return _capability.Activity.FailedAsync(id, result, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost("Activities/{id}/Retry")]
        public async Task RetryAsync(string id, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            await _capability.Activity.RetryAsync(id, cancellationToken);
        }
    }
}
