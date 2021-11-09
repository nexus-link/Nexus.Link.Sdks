using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IActivityVersionParameterTable : ICreate<ActivityVersionParameterRecordCreate, ActivityVersionParameterRecord, Guid>
    {
        Task<ActivityVersionParameterRecord> ReadAsync(Guid activityVersionId, string name, CancellationToken cancellationToken = default);
        Task<PageEnvelope<ActivityVersionParameterRecord>> ReadAllWithPagingAsync(Guid activityVersionId, int offset, int? limit, CancellationToken cancellationToken = default);
    }
}