using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class WorkflowInstanceRestClient : CrudManyToOneRestClient2<WorkflowInstanceCreate, WorkflowInstance, string>, IWorkflowInstanceService
    {
        public WorkflowInstanceRestClient(IHttpSender httpSender) : base(httpSender, "workflow-versions", "instances")
        {
        }
    }
}