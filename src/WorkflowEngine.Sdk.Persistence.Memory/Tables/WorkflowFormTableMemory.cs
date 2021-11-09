using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class WorkflowFormTableMemory : CrudMemory<WorkflowFormRecordCreate, WorkflowFormRecord, Guid>, IWorkflowFormTable
    {
        public WorkflowFormTableMemory()
        {
            UniqueConstraintMethods += item => new { item.CapabilityName, item.Title };
        }
    }
}