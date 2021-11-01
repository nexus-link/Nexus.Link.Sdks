using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Administration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Administration
{
    /// <inheritdoc cref="IWorkflowService" />
    [ApiController]
    [Route("api/v1/workflows/Administration")]
    public class WorkflowsController : Controllers.Administration.WorkflowsController
    {
        /// <inheritdoc />
        public WorkflowsController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
