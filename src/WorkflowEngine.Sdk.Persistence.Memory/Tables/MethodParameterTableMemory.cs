using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using WorkflowEngine.Persistence.Abstract.Entities;
using WorkflowEngine.Persistence.Abstract.Tables;

namespace WorkflowEngine.Persistence.Memory.Tables
{
    public class MethodParameterTableMemory : DependentToMasterMemory<MethodParameterRecordCreate, MethodParameterRecord, Guid, string>, IMethodParameterTable
    {
    }
}