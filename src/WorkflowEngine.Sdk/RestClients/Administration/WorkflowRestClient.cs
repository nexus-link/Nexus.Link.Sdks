using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{
    /// <inheritdoc cref="IWorkflowService" />
    public class WorkflowRestClient : CrudRestClient<Workflow, string>, IWorkflowService
    {
        /// <summary>
        /// Controller
        /// </summary>
        public WorkflowRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Workflows"))
        {
        }

        /// <inheritdoc />
        public async Task CancelAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(workflowInstanceId, nameof(workflowInstanceId));

            var relativeUrl = $"{WebUtility.UrlEncode(workflowInstanceId)}/Cancel";
            await PostNoResponseContentAsync(relativeUrl, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task RetryHaltedAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(workflowInstanceId, nameof(workflowInstanceId));

            var relativeUrl = $"{WebUtility.UrlEncode(workflowInstanceId)}/RetryHalted";
            await PostNoResponseContentAsync(relativeUrl, cancellationToken: cancellationToken);
        }
    }
}
