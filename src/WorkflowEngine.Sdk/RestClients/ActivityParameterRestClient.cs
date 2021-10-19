using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class ActivityParameterRestClient : CrudDependentToMasterRestClient<ActivityParameterCreate, ActivityParameter, string, string>, IActivityParameterService
    {
        public ActivityParameterRestClient(IHttpSender httpSender) : base("ActivityVersions", "Parameters", httpSender)
        {
        }
    }
}