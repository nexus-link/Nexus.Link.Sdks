using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IWorkflowVersionTable :
        ICreate<WorkflowVersionRecordCreate, WorkflowVersionRecord, Guid>
    {
        Task<WorkflowVersionRecord> ReadAsync(Guid workflowFormId, int majorVersion, CancellationToken cancellationToken = default);
        Task UpdateAsync(Guid workflowFormId, int majorVersion, WorkflowVersionRecord @record, CancellationToken cancellationToken = default);
    }
}