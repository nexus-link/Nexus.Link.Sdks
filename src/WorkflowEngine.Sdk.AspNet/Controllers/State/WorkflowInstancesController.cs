using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.State
{
    /// <inheritdoc cref="IWorkflowInstanceService" />
    public abstract class WorkflowInstancesController : ControllerBase, IWorkflowInstanceService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected WorkflowInstancesController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpPost("")]
        public async Task CreateWithSpecifiedIdAsync(string instanceId, WorkflowInstanceCreate item, CancellationToken cancellationToken = default)
        {
            await CreateWithSpecifiedIdAndReturnAsync(instanceId, item, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost("ReturnCreated")]
        public async Task<WorkflowInstance> CreateWithSpecifiedIdAndReturnAsync(string instanceId, WorkflowInstanceCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            return await _capability.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(instanceId, item, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("")]
        public async Task<WorkflowInstance> ReadAsync(string instanceId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));

            var result = await _capability.WorkflowInstance.ReadAsync(instanceId, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpPut("")]
        public async Task UpdateAsync(string instanceId, WorkflowInstance item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            await _capability.WorkflowInstance.UpdateAsync(instanceId, item, cancellationToken);
        }
    }
}
