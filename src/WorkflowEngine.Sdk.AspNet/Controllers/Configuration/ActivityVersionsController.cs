using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Configuration
{
    /// <inheritdoc cref="IActivityVersionService" />
    public abstract class ActivityVersionsController : ControllerBase, IActivityVersionService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected ActivityVersionsController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpPost("")]
        public async Task<string> CreateAsync(ActivityVersionCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            var id = await _capability.ActivityVersion.CreateAsync(item, cancellationToken);
            FulcrumAssert.IsNotNullOrWhiteSpace(id, CodeLocation.AsString());
            return id;
        }

        /// <inheritdoc />
        [HttpGet("")]
        public async Task<ActivityVersion> FindUniqueAsync(string workflowVersionId, string activityFormId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            ServiceContract.RequireNotNullOrWhiteSpace(activityFormId, nameof(activityFormId));
            
            var result = await _capability.ActivityVersion.FindUniqueAsync(workflowVersionId, activityFormId, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpPut("{id}")]
        public async Task UpdateAsync(string id, ActivityVersion item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            await _capability.ActivityVersion.UpdateAsync(id, item, cancellationToken);
        }
    }
}
