using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{

    public class WorkflowRestClient : CrudRestClient<Workflow, string>, IWorkflowService
    {
        public WorkflowRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Workflows"))
        {
        }
    }
}