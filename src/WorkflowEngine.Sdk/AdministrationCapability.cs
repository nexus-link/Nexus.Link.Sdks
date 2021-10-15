using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Services;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    public class AdministrationCapability : IAdministrationCapability
    {
        public AdministrationCapability(IWorkflowService workflowService, IRequestResponseService requestResponseService, IRuntimeTables runtimeTables)
        {
            WorkflowAdministrationService = new WorkflowAdministrationService(workflowService, requestResponseService, runtimeTables);
        }

        public IWorkflowAdministrationService WorkflowAdministrationService { get; }
    }
}