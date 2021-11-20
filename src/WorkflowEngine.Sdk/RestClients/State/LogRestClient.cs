using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Log = Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State.Log;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.State
{
    public class LogRestClient : CrudRestClient<LogCreate, Log, string>, ILogService
    {
        public LogRestClient(IHttpSender httpSender) : base(httpSender)
        {
        }

        /// <inheritdoc />
        public Task<PageEnvelope<Log>> ReadWorkflowChildrenWithPagingAsync(string workflowInstanceId, bool alsoActivityChildren, int offset,
            int? limit = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));

            var relativeUrl = $"WorkflowInstances/{WebUtility.UrlEncode(workflowInstanceId)}/Logs/alsoActivityChildren={alsoActivityChildren}";
            return GetAsync<PageEnvelope<Log>>(relativeUrl, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<Log>> ReadActivityChildrenWithPagingAsync(string workflowInstanceId, string activityFormId, int offset,
            int? limit = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            InternalContract.RequireNotNullOrWhiteSpace(activityFormId, nameof(activityFormId));

            var relativeUrl = $"WorkflowInstances/{WebUtility.UrlEncode(workflowInstanceId)}/ActivityForms/{WebUtility.UrlEncode(activityFormId)}/Logs";
            return GetAsync<PageEnvelope<Log>>(relativeUrl, null, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteWorkflowChildrenAsync(string workflowInstanceId, LogSeverityLevel? threshold = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task DeleteActivityChildrenAsync(string workflowInstanceId, string activityFormId, LogSeverityLevel? threshold = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }
    }
}