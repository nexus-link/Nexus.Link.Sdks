using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Services.State
{
    public class LogService : ILogService
    {
        private readonly IRuntimeTables _runtimeTables;

        public LogService(IRuntimeTables runtimeTables)
        {
            _runtimeTables = runtimeTables;
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(LogCreate item, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var record = new LogRecordCreate().From(item);
            var options = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
            using var scope = new TransactionScope(TransactionScopeOption.Suppress, options, TransactionScopeAsyncFlowOption.Enabled);
            var idAsGuid = await _runtimeTables.Log.CreateAsync(record, cancellationToken);
            scope.Complete();
            return idAsGuid.ToLowerCaseString();
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<Log>> ReadWorkflowChildrenWithPagingAsync(string workflowInstanceId, bool alsoActivityChildren, int offset,
            int? limit = null, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            var idAsGuid = workflowInstanceId.ToGuid();
            var page = await _runtimeTables.Log.ReadWorkflowChildrenWithPagingAsync(idAsGuid, alsoActivityChildren,
                offset, limit, cancellationToken);
            return new PageEnvelope<Log>(page.PageInfo, page.Data.Select(i => new Log().From(i)));
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<Log>> ReadActivityChildrenWithPagingAsync(string activityInstanceId, int offset, int? limit = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(activityInstanceId, nameof(activityInstanceId));
            var idAsGuid = activityInstanceId.ToGuid();
            var page = await _runtimeTables.Log.ReadActivityChildrenWithPagingAsync(idAsGuid,
                offset, limit, cancellationToken);
            return new PageEnvelope<Log>(page.PageInfo, page.Data.Select(i => new Log().From(i)));
        }
    }
}