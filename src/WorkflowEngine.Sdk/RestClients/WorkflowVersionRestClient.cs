using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class WorkflowVersionRestClient : CrudDependentToMasterRestClient<WorkflowVersionCreate, WorkflowVersion, string, int>, IWorkflowVersionService
    {
        public WorkflowVersionRestClient(IHttpSender httpSender) : base("WorkflowForms", "Versions", httpSender)
        {
        }
    }
}