using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class ActivityFormRestClient : CrudManyToOneRestClient2<ActivityFormCreate, ActivityForm, string>, IActivityFormService
    {
        public ActivityFormRestClient(IHttpSender httpSender) : base(httpSender, "workflows", "activities")
        {
        }
    }
}