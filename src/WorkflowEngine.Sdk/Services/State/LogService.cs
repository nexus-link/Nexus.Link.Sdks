using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Log = Nexus.Link.Capabilities.WorkflowState.Abstract.Entities.Log;

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
            var workflowInstanceId = await _runtimeTables.Log.CreateAsync(record, cancellationToken);
            scope.Complete();
            return workflowInstanceId.ToGuidString();
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
        public async Task<PageEnvelope<Log>> ReadActivityChildrenWithPagingAsync(string workflowInstanceId, string activityFormId, int offset,
            int? limit = null, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            InternalContract.RequireNotNullOrWhiteSpace(activityFormId, nameof(activityFormId));
            var page = await _runtimeTables.Log.ReadActivityChildrenWithPagingAsync(workflowInstanceId.ToGuid(),
                activityFormId.ToGuid(), 
                offset, limit, cancellationToken);
            return new PageEnvelope<Log>(page.PageInfo, page.Data.Select(i => new Log().From(i)));
        }

        /// <inheritdoc />
        public async Task DeleteWorkflowChildrenAsync(string workflowInstanceId, LogSeverityLevel? threshold = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            var options = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
            using var scope = new TransactionScope(TransactionScopeOption.Suppress, options, TransactionScopeAsyncFlowOption.Enabled);
            await _runtimeTables.Log.DeleteWorkflowChildrenAsync(workflowInstanceId.ToGuid(), threshold == null ? null : (int) threshold, cancellationToken);
            scope.Complete();
        }

        /// <inheritdoc />
        public async Task DeleteActivityChildrenAsync(string workflowInstanceId, string activityFormId, LogSeverityLevel? threshold = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
            InternalContract.RequireNotNullOrWhiteSpace(activityFormId, nameof(activityFormId));
            var options = new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted };
            using var scope = new TransactionScope(TransactionScopeOption.Suppress, options, TransactionScopeAsyncFlowOption.Enabled);
            await _runtimeTables.Log.DeleteActivityChildrenAsync(workflowInstanceId.ToGuid(), activityFormId.ToGuid(), threshold == null ? null : (int) threshold, cancellationToken);
            scope.Complete();
        }
    }
}