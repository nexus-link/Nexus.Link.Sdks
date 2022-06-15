using System.Collections.Generic;
using Nexus.Link.Contracts.Misc.Sdk.Authentication;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Tables
{
    /// <inheritdoc cref="IHashTable" />
    public class HashTableSql : CrudSql<HashRecordCreate, HashRecord>, IHashTable
    {
        /// <inheritdoc />
        public HashTableSql(IDatabaseOptions options) : base(options, new SqlTableMetadata
        {
            TableName = "Hash",
            CreatedAtColumnName = nameof(HashRecord.RecordCreatedAt),
            UpdatedAtColumnName = nameof(HashRecord.RecordUpdatedAt),
            RowVersionColumnName = nameof(HashRecord.RecordVersion),
            CustomColumnNames = new List<string>
            {
                nameof(HashRecord.Salt),
                nameof(HashRecord.Hash),
            },
            OrderBy = new List<string> { nameof(HashRecord.RecordCreatedAt) },
            UpdateCanUseOutput = true
        })
        {
        }
    }
}