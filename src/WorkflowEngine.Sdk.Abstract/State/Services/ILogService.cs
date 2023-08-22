using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Log = Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities.Log;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

public interface ILogService: ICreate<LogCreate, Entities.Log, string>
{
    Task<PageEnvelope<Log>> ReadWorkflowChildrenWithPagingAsync(string workflowInstanceId,
        bool alsoActivityChildren, int offset, int? limit = null,
        CancellationToken cancellationToken = default);

    Task<PageEnvelope<Log>> ReadActivityChildrenWithPagingAsync(string workflowInstanceId, string activityFormId, int offset,
        int? limit = null,
        CancellationToken cancellationToken = default);
        
    Task DeleteWorkflowChildrenAsync(string workflowInstanceId, LogSeverityLevel? threshold = null, CancellationToken cancellationToken = default);
    Task DeleteActivityChildrenAsync(string workflowInstanceId, string activityFormId, LogSeverityLevel? threshold = null, CancellationToken cancellationToken = default);
}