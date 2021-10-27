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
    /// <inheritdoc cref="IActivityFormService" />
    public abstract class ActivityFormsController : ControllerBase, IActivityFormService
    {
        private readonly IWorkflowCapability _capability;

        protected ActivityFormsController(IWorkflowCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpPost("")]
        public async Task CreateWithSpecifiedIdAsync(string id, ActivityFormCreate item, CancellationToken cancellationToken = default)
        {
            await CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost("ReturnCreated")]
        public async Task<ActivityForm> CreateWithSpecifiedIdAndReturnAsync(string id, ActivityFormCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            return await _capability.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("")]
        public async Task<ActivityForm> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var result = await _capability.ActivityForm.ReadAsync(id, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpPut("")]
        public async Task UpdateAsync(string id, ActivityForm item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            
            await _capability.ActivityForm.UpdateAsync(id, item, cancellationToken);
        }
    }
}
