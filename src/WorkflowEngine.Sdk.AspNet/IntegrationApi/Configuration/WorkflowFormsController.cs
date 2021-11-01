using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.IntegrationApi.Configuration
{
    /// <inheritdoc cref="IWorkflowFormService" />
    [ApiController]
    [Route("api/v1/workflows/Configuration/WorkflowForms/{id}")]
    public class WorkflowFormsController : Controllers.Configuration.WorkflowFormsController
    {
        /// <inheritdoc />
        public WorkflowFormsController(IWorkflowMgmtCapability capability) : base(capability)
        {
        }
    }
}
