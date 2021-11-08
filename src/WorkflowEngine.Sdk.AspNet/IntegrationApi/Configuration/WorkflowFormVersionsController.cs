﻿using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Configuration
{
    /// <inheritdoc cref="IWorkflowParameterService" />
    [ApiController]
    [Route("api/v1/workflows/Configuration/WorkflowForms/{workflowFormId}/Versions/{majorVersion}")]
    public abstract class WorkflowFormVersionsController : Controllers.Configuration.WorkflowFormVersionsController
    {
        /// <inheritdoc />
        protected WorkflowFormVersionsController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}