using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Configuration
{
    /// <inheritdoc cref="IWorkflowParameterService" />
    public abstract class WorkflowFormVersionsController : ControllerBase, IWorkflowVersionService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected WorkflowFormVersionsController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpPost("ReturnResult")]
        public async Task<WorkflowVersion> CreateWithSpecifiedIdAndReturnAsync(string workflowVersionId, WorkflowVersionCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            
            return await _capability.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(workflowVersionId, item, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("")]
        public async Task<WorkflowVersion> ReadAsync(string workflowVersionId, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            
            var result = await _capability.WorkflowVersion.ReadAsync(workflowVersionId, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        public async Task<WorkflowVersion> UpdateAndReturnAsync(string workflowVersionId, WorkflowVersion item,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            return await _capability.WorkflowVersion.UpdateAndReturnAsync(workflowVersionId, item, cancellationToken);
        }
    }
}
