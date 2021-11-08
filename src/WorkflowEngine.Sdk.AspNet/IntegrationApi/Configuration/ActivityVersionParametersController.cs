﻿using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Configuration
{
    /// <inheritdoc cref="IActivityParameterService" />
    [ApiController]
    [Route("api/v1/workflows/Configuration/ActivityVersions/{activityVersionId}/Parameters")]
    public abstract class ActivityVersionParametersController : Controllers.Configuration.ActivityVersionParametersController
    {
        /// <inheritdoc />
        protected ActivityVersionParametersController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}