﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Runtime
{
    /// <inheritdoc cref="IActivityInstanceService" />
    public abstract class ActivityInstancesController : ControllerBase, IActivityInstanceService
    {
        private readonly IWorkflowMgmtCapability _capability;

        protected ActivityInstancesController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpPost("ReturnCreated")]
        public async Task<ActivityInstance> CreateAndReturnAsync(ActivityInstanceCreate item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            var result = await _capability.ActivityInstance.CreateAndReturnAsync(item, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
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
        [HttpPut("{id}/ReturnUpdated")]
        public async Task<ActivityInstance> UpdateAndReturnAsync(string id, ActivityInstance item, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));

            var result = await _capability.ActivityInstance.UpdateAndReturnAsync(id, item, cancellationToken);
            FulcrumAssert.IsValidated(result, CodeLocation.AsString());
            return result;
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

        /// <inheritdoc />
        [HttpPost("{id}/Success")]
        public async Task SuccessAsync(string id, ActivityInstanceSuccessResult result, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(result, nameof(result));
            ServiceContract.RequireValidated(result, nameof(result));

            await _capability.ActivityInstance.SuccessAsync(id, result, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost("{id}/Failed")]
        public async Task FailedAsync(string id, ActivityInstanceFailedResult result, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(result, nameof(result));
            ServiceContract.RequireValidated(result, nameof(result));

            await _capability.ActivityInstance.FailedAsync(id, result, cancellationToken);
        }
    }
}
