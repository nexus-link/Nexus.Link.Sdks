using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Configuration
{
    /// <inheritdoc cref="IWorkflowParameterService" />
    [ApiController]
    [Route("api/v1/workflows/Configuration/WorkflowVersions/{workflowVersionId}/Parameters")]
    public abstract class WorkflowVersionParametersController : Controllers.Configuration.WorkflowVersionParametersController
    {
        /// <inheritdoc />
        protected WorkflowVersionParametersController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
