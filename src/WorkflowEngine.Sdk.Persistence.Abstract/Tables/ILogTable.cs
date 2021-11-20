using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface ILogTable: ICreate<LogRecordCreate, LogRecord, Guid>
    {
        Task<PageEnvelope<LogRecord>> ReadWorkflowChildrenWithPagingAsync(
            Guid workflowInstanceId, bool alsoActivityChildren, 
            int offset, int? limit = null,
            CancellationToken cancellationToken = default);

        Task<PageEnvelope<LogRecord>> ReadActivityChildrenWithPagingAsync(
            Guid workflowInstanceId, Guid activityFormId, 
            int offset, int? limit = null,
            CancellationToken cancellationToken = default);

        Task DeleteWorkflowChildrenAsync(Guid workflowInstanceId, int? threshold = null, CancellationToken cancellationToken = default);

        Task DeleteActivityChildrenAsync(Guid workflowInstanceId, Guid activityFormId, int? threshold = null, CancellationToken cancellationToken = default);
    }
}