using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Administration
{
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
