using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Configuration
{
    public class WorkflowParameterRestClient : CrudDependentToMasterRestClient<WorkflowParameterCreate, WorkflowParameter, string, string>, IWorkflowParameterService
    {
        public WorkflowParameterRestClient(IHttpSender httpSender) : base("WorkflowVersions", "Parameters", httpSender)
        {
        }
    }
}