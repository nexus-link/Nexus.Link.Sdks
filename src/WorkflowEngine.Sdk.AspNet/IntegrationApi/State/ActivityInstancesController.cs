using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.State
{
    /// <inheritdoc cref="IActivityInstanceService" />
    [ApiController]
    [Route("api/v1/workflows/State/ActivityInstances")]
    public abstract class ActivityInstancesController : Controllers.State.ActivityInstancesController
    {
        /// <inheritdoc />
        protected ActivityInstancesController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
