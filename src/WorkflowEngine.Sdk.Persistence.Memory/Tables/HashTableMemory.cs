using System;
using Nexus.Link.Contracts.Misc.Sdk.Authentication;
using Nexus.Link.Libraries.Crud.MemoryStorage;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables;

/// <inheritdoc cref="IReentryTokenTable" />
public class HashTableMemory : CrudMemory<HashRecordCreate, HashRecord, Guid>, IHashTable
{
}