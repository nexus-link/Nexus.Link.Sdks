using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Runtime
{
    /// <inheritdoc cref="IActivityInstanceService" />
    [ApiController]
    [Route("api/v1/workflows/Runtime/ActivityInstances")]
    public class ActivityInstancesController : Controllers.Runtime.ActivityInstancesController
    {
        /// <inheritdoc />
        public ActivityInstancesController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
