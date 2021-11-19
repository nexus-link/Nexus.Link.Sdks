using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class LogTableMemory : CrudMemory<LogRecordCreate, LogRecord, Guid>, ILogTable
    {
        public LogTableMemory()
        {
        }

        /// <inheritdoc />
        public Task<PageEnvelope<LogRecord>> ReadWorkflowChildrenWithPagingAsync(Guid workflowInstanceId, bool alsoActivityChildren, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(workflowInstanceId, nameof(workflowInstanceId));
            if (alsoActivityChildren)
            {
                return SearchAsync(new SearchDetails<LogRecord>(
                    new
                    {
                        WorkflowInstanceId = workflowInstanceId
                    }), offset, limit, cancellationToken);
            }
            return SearchAsync(new SearchDetails<LogRecord>(
                new
                {
                    WorkflowInstanceId = workflowInstanceId,
                    ActivityFormId = (Guid?)null
                }), offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<LogRecord>> ReadActivityChildrenWithPagingAsync(Guid activityFormId, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(activityFormId, nameof(activityFormId));
            return SearchAsync(new SearchDetails<LogRecord>(
                new
                {
                    ActivityFormId = activityFormId
                }), offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteWorkflowChildrenAsync(Guid workflowInstanceId, CancellationToken cancellationToken)
        {
            var logs = await StorageHelper.ReadPagesAsync<LogRecord>(
                (o, ct) => ReadWorkflowChildrenWithPagingAsync(workflowInstanceId, true, o, null, ct), int.MaxValue, cancellationToken);
            await Task.WhenAll(logs.Select(l => DeleteAsync(l.Id, cancellationToken)));
        }

        /// <inheritdoc />
        public async Task DeleteActivityChildrenAsync(Guid activityFormId, CancellationToken cancellationToken)
        {
            var logs = await StorageHelper.ReadPagesAsync<LogRecord>(
                (o, ct) => ReadActivityChildrenWithPagingAsync(activityFormId, o, null, ct), int.MaxValue, cancellationToken);
            await Task.WhenAll(logs.Select(l => DeleteAsync(l.Id, cancellationToken)));
        }
    }
}