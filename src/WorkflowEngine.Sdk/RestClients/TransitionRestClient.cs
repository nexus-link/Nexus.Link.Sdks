using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients
{
    public class TransitionRestClient : CrudManyToOneRestClient2<TransitionCreate, Transition, string>, ITransitionService
    {
        public TransitionRestClient(IHttpSender httpSender) : base(httpSender, "WorkflowVersions", "Transitions")
        {
        }

        /// <inheritdoc />
        public Task<Transition> FindUniqueAsync(string workflowVersionId, TransitionUnique transition, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"WorkflowVersions/{WebUtility.UrlEncode(workflowVersionId)}/Transitions";
            return PostAsync<Transition, TransitionUnique>(relativeUrl, transition, cancellationToken: cancellationToken);
        }
    }
}