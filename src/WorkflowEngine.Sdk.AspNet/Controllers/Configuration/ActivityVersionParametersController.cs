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
    /// <inheritdoc cref="IActivityParameterService" />
    public abstract class ActivityVersionParametersController : ControllerBase, IActivityParameterService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected ActivityVersionParametersController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpPost("{parameterName}")]
        public async Task CreateWithSpecifiedIdAsync(string activityVersionId, string parameterName, ActivityParameterCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(activityVersionId, nameof(activityVersionId));
            ServiceContract.RequireNotNullOrWhiteSpace(parameterName, nameof(parameterName));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            await _capability.ActivityParameter.CreateWithSpecifiedIdAsync(activityVersionId, parameterName, item, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("{parameterName}")]
        public async Task<ActivityParameter> ReadAsync(string activityVersionId, string parameterName, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(activityVersionId, nameof(activityVersionId));
            ServiceContract.RequireNotNullOrWhiteSpace(parameterName, nameof(parameterName));
            
            var result = await _capability.ActivityParameter.ReadAsync(activityVersionId, parameterName, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
        }

        /// <inheritdoc />
        [HttpGet("")]
        public async Task<PageEnvelope<ActivityParameter>> ReadChildrenWithPagingAsync(string activityVersionId, int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(activityVersionId, nameof(activityVersionId));
            
            var result = await _capability.ActivityParameter.ReadChildrenWithPagingAsync(activityVersionId, offset, limit, cancellationToken);
            FulcrumAssert.IsValidated(result.Data, CodeLocation.AsString());
            return result;
        }
    }
}
