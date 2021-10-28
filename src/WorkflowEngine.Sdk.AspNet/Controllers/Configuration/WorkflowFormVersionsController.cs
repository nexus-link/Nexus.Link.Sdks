using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Configuration
{
    /// <inheritdoc cref="IWorkflowParameterService" />
    public abstract class WorkflowFormVersionsController : ControllerBase, IWorkflowVersionService
    {
        private readonly IWorkflowCapability _capability;

        protected WorkflowFormVersionsController(IWorkflowCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpPost("")]
        public async Task CreateWithSpecifiedIdAsync(string workflowFormId, int majorVersion, WorkflowVersionCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowFormId, nameof(workflowFormId));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            
            await _capability.WorkflowVersion.CreateWithSpecifiedIdAsync(workflowFormId, majorVersion, item, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("")]
        public async Task<WorkflowVersion> ReadAsync(string workflowFormId, int majorVersion, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowFormId, nameof(workflowFormId));
            
            var result = await _capability.WorkflowVersion.ReadAsync(workflowFormId, majorVersion, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpPut("")]
        public async Task UpdateAsync(string workflowFormId, int majorVersion, WorkflowVersion item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowFormId, nameof(workflowFormId));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            ServiceContract.RequireAreEqual(workflowFormId, item.WorkflowFormId, $"{nameof(item)}.{nameof(item.WorkflowFormId)}");
            ServiceContract.RequireAreEqual(majorVersion, item.MajorVersion, $"{nameof(item)}.{nameof(item.MajorVersion)}");

            await _capability.WorkflowVersion.UpdateAsync(workflowFormId, majorVersion, item, cancellationToken);
        }
    }
}
