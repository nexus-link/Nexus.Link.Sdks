using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;

namespace WorkflowEngine.Sdk.RestClients
{
    public class WorkflowVersionRestClient : CrudDependentToMasterRestClient<WorkflowVersionCreate, WorkflowVersion, string, int>, IWorkflowVersionService
    {
        public WorkflowVersionRestClient(IHttpSender httpSender) : base("workflows", "Versions", httpSender)
        {
        }
    }
}