using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Administration
{
    /// <inheritdoc cref="IWorkflowService" />
    public abstract class FormsController : ControllerBase, IFormService, IFormOverviewService
    {
        private readonly IWorkflowMgmtCapability _capability;

        /// <summary>
        /// Controller
        /// </summary>
        protected FormsController(IWorkflowMgmtCapability capability)
        {
            _capability = capability;
        }

        /// <inheritdoc />
        [HttpGet("FormOverviews")]
        public async Task<IList<WorkflowFormOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset instancesFrom, DateTimeOffset instancesTo, CancellationToken cancellationToken = default)
        {
            ServiceContract.Require(instancesTo > instancesFrom, $"{nameof(instancesTo)} must be greater than {instancesFrom}");

            return await _capability.FormOverview.ReadByIntervalWithPagingAsync(instancesFrom, instancesTo, cancellationToken);
        }


        /// <inheritdoc />
        [HttpGet("Forms/{id}")]
        public async Task<WorkflowForm> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return await _capability.Form.ReadAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("Forms")]
        public async Task<PageEnvelope<WorkflowForm>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit.HasValue) ServiceContract.RequireGreaterThanOrEqualTo(1, limit.Value, nameof(limit));

            return await _capability.Form.ReadAllWithPagingAsync(offset, limit, cancellationToken);
        }
    }
}
