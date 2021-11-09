using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Configuration
{
    public class WorkflowVersionRestClient : CrudRestClient<WorkflowVersionCreate, WorkflowVersion, string>, IWorkflowVersionService
    {
        public WorkflowVersionRestClient(IHttpSender httpSender) : base(httpSender)
        {
        }
    }
}