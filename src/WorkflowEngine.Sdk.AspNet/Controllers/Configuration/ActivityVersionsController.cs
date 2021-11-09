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
        [HttpPost("{id}")]
        public async Task<ActivityVersion> CreateWithSpecifiedIdAndReturnAsync(string id, ActivityVersionCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            return await _capability.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("{id}")]
        public async Task<ActivityVersion> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            
            var result = await _capability.ActivityVersion.ReadAsync(id, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpPut("{id}")]
        public async Task<ActivityVersion> UpdateAndReturnAsync(string id, ActivityVersion item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            return await _capability.ActivityVersion.UpdateAndReturnAsync(id, item, cancellationToken);
        }
    }
}
