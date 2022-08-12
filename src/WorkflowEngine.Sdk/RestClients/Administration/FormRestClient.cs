using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{
    /// <inheritdoc cref="IFormService" />
    public class FormRestClient : CrudRestClient<WorkflowForm, string>, IFormService
    {
        /// <summary>
        /// Controller
        /// </summary>
        public FormRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Forms"))
        {
        }
    }
}
