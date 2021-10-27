using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Runtime
{
    /// <inheritdoc cref="IActivityInstanceService" />
    public abstract class ActivityInstancesController : ControllerBase, IActivityInstanceService
    {
        private readonly IWorkflowCapability _capability;

        protected ActivityInstancesController(IWorkflowCapability capability)
        {
            _capability = capability;
        }

     /// <inheritdoc />
        [HttpPost("")]
        public async Task<string> CreateAsync(ActivityInstanceCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            var result = await _capability.ActivityInstance.CreateAsync(item, cancellationToken);
            FulcrumAssert.IsNotNullOrWhiteSpace(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpGet("{id}")]
        public async Task<ActivityInstance> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var result = await _capability.ActivityInstance.ReadAsync(id, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpPut("{id}")]
        public async Task UpdateAsync(string id, ActivityInstance item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            await _capability.ActivityInstance.UpdateAsync(id, item ,cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost("FindUnique")]
        public async Task<ActivityInstance> FindUniqueAsync(ActivityInstanceUnique item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            var result = await _capability.ActivityInstance.FindUniqueAsync(item, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }
    }
}
