using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class WorkflowParameterRestClient : CrudDependentToMasterRestClient<WorkflowParameterCreate, WorkflowParameter, string, string>, IWorkflowParameterService
    {
        public WorkflowParameterRestClient(IHttpSender httpSender) : base("WorkflowVersions", "Parameters", httpSender)
        {
        }
    }
}