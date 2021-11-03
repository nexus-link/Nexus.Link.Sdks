using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Configuration
{
    /// <inheritdoc cref="ITransitionService" />
    [ApiController]
    [Route("api/v1/workflows/Configuration/WorkflowVersions/{workflowVersionId}/Transitions")]
    public abstract class WorkflowVersionTransitionsController : Controllers.Configuration.WorkflowVersionTransitionsController
    {
        /// <inheritdoc />
        protected WorkflowVersionTransitionsController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
