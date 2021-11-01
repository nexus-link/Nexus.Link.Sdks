using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Administration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Runtime
{
    /// <inheritdoc cref="IWorkflowService" />
    [ApiController]
    [Route("api/v1/workflows/Runtime/Workflows")]
    public class WorkflowController : Controllers.Runtime.WorkflowController
    {
        /// <inheritdoc />
        public WorkflowController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
