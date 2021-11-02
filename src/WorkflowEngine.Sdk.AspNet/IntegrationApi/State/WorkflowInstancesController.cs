using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.State
{
    /// <inheritdoc cref="IWorkflowInstanceService" />
    [ApiController]
    [Route("api/v1/workflows/State/WorkflowInstances/{instanceId}")]
    public class WorkflowInstancesController : Controllers.State.WorkflowInstancesController
    {
        /// <inheritdoc />
        public WorkflowInstancesController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
