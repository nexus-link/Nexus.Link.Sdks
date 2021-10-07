using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class MethodParameterTableMemory : CrudMemory<MethodParameterRecordCreate, MethodParameterRecord, Guid>, IMethodParameterTable
    {
        /// <inheritdoc />
        public Task<MethodParameterRecord> ReadAsync(Guid xVersionId, string name, CancellationToken cancellationToken = default)
        {
            return FindUniqueAsync(
                new SearchDetails<MethodParameterRecord>(
                    new MethodParameterRecordUnique
                    {
                        XVersionId = xVersionId,
                        Name = name
                    }),
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<MethodParameterRecord>> ReadAllWithPagingAsync(Guid xVersionId, int offset, int? limit, CancellationToken cancellationToken = default)
        {
            return SearchAsync(
                new SearchDetails<MethodParameterRecord>(
                    new
                    {
                        XVersionId = xVersionId
                    }),
                offset, limit, cancellationToken);
        }
    }
}