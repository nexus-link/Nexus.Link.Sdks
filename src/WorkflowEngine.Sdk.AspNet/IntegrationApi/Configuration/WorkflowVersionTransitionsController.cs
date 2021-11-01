using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Configuration
{
    /// <inheritdoc cref="ITransitionService" />
    [ApiController]
    [Route("api/v1/workflows/Configuration/WorkflowVersions/{workflowVersionId}/Transitions")]
    public class WorkflowVersionTransitionsController : Controllers.Configuration.WorkflowVersionTransitionsController
    {
        /// <inheritdoc />
        public WorkflowVersionTransitionsController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
