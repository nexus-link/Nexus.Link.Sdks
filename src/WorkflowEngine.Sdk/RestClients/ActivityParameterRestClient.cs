using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;

namespace WorkflowEngine.Sdk.RestClients
{
    public class ActivityParameterRestClient : CrudDependentToMasterRestClient<ActivityParameterCreate, ActivityParameter, string, string>, IActivityParameterService
    {
        public ActivityParameterRestClient(IHttpSender httpSender) : base("activities", "parameters", httpSender)
        {
        }
    }
}