using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{
    /// <inheritdoc cref="IInstanceService" />
    public class InstanceRestClient : CrudRestClient<WorkflowForm, string>, IInstanceService
    {
        /// <summary>
        /// Controller
        /// </summary>
        public InstanceRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Instances"))
        {
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<WorkflowInstance>> SearchAsync(WorkflowInstanceSearchDetails searchDetails, int offset = 0, int? limit = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(searchDetails, nameof(searchDetails));
            InternalContract.RequireValidated(searchDetails, nameof(searchDetails));

            var relativeUrl = $"?offset={offset}";
            if (limit.HasValue) relativeUrl += $"&limit={limit}";
            return await PostAsync<PageEnvelope<WorkflowInstance>, WorkflowInstanceSearchDetails>(relativeUrl, searchDetails, cancellationToken: cancellationToken);
        }
    }
}
