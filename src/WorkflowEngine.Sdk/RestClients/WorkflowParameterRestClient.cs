using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;

namespace WorkflowEngine.Sdk.RestClients
{
    public class WorkflowParameterRestClient : CrudDependentToMasterRestClient<WorkflowParameterCreate, WorkflowParameter, string, string>, IWorkflowParameterService
    {
        public WorkflowParameterRestClient(IHttpSender httpSender) : base("workflows", "parameters", httpSender)
        {
        }
    }
}