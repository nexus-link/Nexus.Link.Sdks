using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients;

public class WorkflowAdministrationRestClient : CrudRestClient<Workflow, string>, IWorkflowAdministrationService
{
    public WorkflowAdministrationRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Workflows"))
    {
    }
}