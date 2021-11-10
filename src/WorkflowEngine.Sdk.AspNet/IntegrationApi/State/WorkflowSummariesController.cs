using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Administration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.State
{
    /// <inheritdoc cref="IWorkflowService" />
    [ApiController]
    [Route("api/v1/workflows/State/WorkflowSummaries")]
    public abstract class WorkflowSummariesController : Controllers.State.WorkflowSummariesController
    {
        /// <inheritdoc />
        protected WorkflowSummariesController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
