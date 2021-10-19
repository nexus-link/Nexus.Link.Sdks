using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class WorkflowFormRestClient : CrudRestClient<WorkflowFormCreate, WorkflowForm, string>, IWorkflowFormService
    {
        public WorkflowFormRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("WorkflowForms"))
        {
        }
    }
}