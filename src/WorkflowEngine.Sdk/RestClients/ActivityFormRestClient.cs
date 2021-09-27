using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;

namespace WorkflowEngine.Sdk.RestClients
{
    public class ActivityFormRestClient : CrudManyToOneRestClient2<ActivityFormCreate, ActivityForm, string>, IActivityFormService
    {
        public ActivityFormRestClient(IHttpSender httpSender) : base(httpSender, "workflows", "activities")
        {
        }
    }
}