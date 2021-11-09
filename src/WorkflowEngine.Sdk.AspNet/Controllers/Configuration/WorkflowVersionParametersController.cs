using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Configuration
{
    /// <inheritdoc cref="IWorkflowParameterService" />
    public abstract class WorkflowVersionParametersController : ControllerBase, IWorkflowParameterService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected WorkflowVersionParametersController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpPost("{parameterName}")]
        public async Task CreateWithSpecifiedIdAsync(string workflowVersionId, string parameterName, WorkflowParameterCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            await _capability.WorkflowParameter.CreateWithSpecifiedIdAsync(workflowVersionId, parameterName, item, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("{parameterName}")]
        public async Task<WorkflowParameter> ReadAsync(string workflowVersionId, string parameterName, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            
            var result = await _capability.WorkflowParameter.ReadAsync(workflowVersionId, parameterName, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpGet("")]
        public async Task<PageEnvelope<WorkflowParameter>> ReadChildrenWithPagingAsync(string workflowVersionId, int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));
            
            var result = await _capability.WorkflowParameter.ReadChildrenWithPagingAsync(workflowVersionId, offset, limit, cancellationToken);
            FulcrumAssert.IsValidated(result.Data, CodeLocation.AsString());
            return result;
        }
    }
}
