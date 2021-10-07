using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    public class MethodParameterTableSql : CrudSql<MethodParameterRecordCreate, MethodParameterRecord>, IMethodParameterTable
    {
        public MethodParameterTableSql(string connectionString) : base(connectionString, tableMetadata)
        {
        }

        public async Task<MethodParameterRecord> ReadAsync(Guid xVersionId, string name, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<PageEnvelope<MethodParameterRecord>> ReadAllWithPagingAsync(Guid xVersionId, int offset, int? limit, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}