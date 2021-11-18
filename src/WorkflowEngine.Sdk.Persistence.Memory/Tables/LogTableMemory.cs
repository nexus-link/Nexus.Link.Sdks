using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
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
                    ActivityInstanceId = (Guid?)null
                }), offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<LogRecord>> ReadActivityChildrenWithPagingAsync(Guid activityInstanceId, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(activityInstanceId, nameof(activityInstanceId));
            return SearchAsync(new SearchDetails<LogRecord>(
                new
                {
                    ActivityInstanceId = activityInstanceId
                }), offset, limit, cancellationToken);
        }
    }
}