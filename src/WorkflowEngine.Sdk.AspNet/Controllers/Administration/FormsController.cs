﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Controllers.Administration
{
    /// <inheritdoc cref="IWorkflowService" />
    public abstract class FormsController : ControllerBase, IFormOverviewService
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
        [HttpGet("Forms")]
        public async Task<IList<WorkflowFormOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset instancesFrom, DateTimeOffset instancesTo, CancellationToken cancellationToken = default)
        {
            ServiceContract.Require(instancesTo > instancesFrom, $"{nameof(instancesTo)} must be greater than {instancesFrom}");

            return await _capability.FormOverview.ReadByIntervalWithPagingAsync(instancesFrom, instancesTo, cancellationToken);
        }
    }
}