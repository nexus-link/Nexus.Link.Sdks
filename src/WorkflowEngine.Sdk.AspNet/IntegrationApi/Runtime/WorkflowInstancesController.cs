using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Runtime
{
    /// <inheritdoc cref="IWorkflowInstanceService" />
    [ApiController]
    [Route("api/v1/workflows/Runtime/WorkflowInstances/{instanceId}")]
    public class WorkflowInstancesController : Controllers.Runtime.WorkflowInstancesController
    {
        /// <inheritdoc />
        public WorkflowInstancesController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
