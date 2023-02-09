using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{
    /// <inheritdoc cref="IVersionService" />
    public class VersionRestClient : CrudDependentToMasterRestClient<WorkflowVersion, string, string>, IVersionService
    {
        /// <summary>
        /// Controller
        /// </summary>
        public VersionRestClient(IHttpSender httpSender) : base("Forms", "Versions", httpSender)
        {
        }
    }
}
