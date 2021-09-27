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
        public TransitionRestClient(IHttpSender httpSender) : base(httpSender, "workflow-versions", "transitions")
        {
        }

        /// <inheritdoc />
        public Task<Transition> FindUniqueAsync(TransitionCreate transition, CancellationToken cancellationToken = default)
        {
            return PostAsync<Transition, TransitionCreate>($"transitions/find-unique", transition, cancellationToken: cancellationToken);
        }
    }
}