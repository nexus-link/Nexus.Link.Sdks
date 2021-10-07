using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IMethodParameterTable: ICreate<MethodParameterRecordCreate, MethodParameterRecord, Guid>
    {
        Task<MethodParameterRecord> ReadAsync(Guid xVersionId, string name, CancellationToken cancellationToken = default);
        Task<PageEnvelope<MethodParameterRecord>> ReadAllWithPagingAsync(Guid xVersionId, int offset, int? limit, CancellationToken cancellationToken = default);
    }
}