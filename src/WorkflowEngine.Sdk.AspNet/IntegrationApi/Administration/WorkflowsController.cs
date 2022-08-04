using System;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Administration
{
    [Obsolete("We don't go through the Integration API anymore, but directly towards each capability provider.")]
    /// <inheritdoc cref="IWorkflowService" />
    [ApiController]
    [Route("api/v1/workflows/Administration")]
    public abstract class WorkflowsController : Controllers.Administration.WorkflowsController
    {
        /// <inheritdoc />
        protected WorkflowsController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
