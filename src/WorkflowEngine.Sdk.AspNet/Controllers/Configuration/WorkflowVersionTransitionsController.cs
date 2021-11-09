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
    /// <inheritdoc cref="ITransitionService" />
    public abstract class WorkflowVersionTransitionsController : ControllerBase, ITransitionService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected WorkflowVersionTransitionsController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpPost("")]
        public async Task<string> CreateChildAsync(string workflowVersionId, TransitionCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            var id = await _capability.Transition.CreateChildAsync(workflowVersionId, item, cancellationToken);
            FulcrumAssert.IsNotNullOrWhiteSpace(id, CodeLocation.AsString());
            return id;
        }

        /// <inheritdoc />
        [HttpPost("FindUnique")]
        public async Task<Transition> FindUniqueAsync(string workflowVersionId, TransitionUnique item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            var result = await _capability.Transition.FindUniqueAsync(workflowVersionId, item, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpGet("")]
        public async Task<PageEnvelope<Transition>> ReadChildrenWithPagingAsync(string workflowVersionId, int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(workflowVersionId, nameof(workflowVersionId));

            var result = await _capability.Transition.ReadChildrenWithPagingAsync(workflowVersionId, offset, limit, cancellationToken);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsValidated(result.Data, CodeLocation.AsString());
            return result;
        }
    }
}
