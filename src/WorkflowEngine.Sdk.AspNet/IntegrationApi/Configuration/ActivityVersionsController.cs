using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Configuration
{
    /// <inheritdoc cref="IActivityVersionService" />
    [ApiController]
    [Route("api/v1/workflows/Configuration/ActivityVersions")]
    public abstract class ActivityVersionsController : Controllers.Configuration.ActivityVersionsController
    {
        /// <inheritdoc />
        protected ActivityVersionsController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
