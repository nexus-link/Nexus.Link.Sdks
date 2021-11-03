using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Configuration
{
    /// <inheritdoc cref="IActivityFormService" />
    [ApiController]
    [Route("api/v1/workflows/Configuration/ActivityForms/{id}")]
    public abstract class ActivityFormsController : Controllers.Configuration.ActivityFormsController
    {
        /// <inheritdoc />
        protected ActivityFormsController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
