using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

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
