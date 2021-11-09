using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IWorkflowVersionParameterTable : ICreate<WorkflowVersionParameterRecordCreate, WorkflowVersionParameterRecord, Guid>
    {
        Task<WorkflowVersionParameterRecord> ReadAsync(Guid workflowVersionId, string name, CancellationToken cancellationToken = default);
        Task<PageEnvelope<WorkflowVersionParameterRecord>> ReadAllWithPagingAsync(Guid workflowVersionId, int offset, int? limit, CancellationToken cancellationToken = default);
    }
}