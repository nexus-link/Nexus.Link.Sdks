using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using WorkflowEngine.Persistence.Abstract.Entities;
using WorkflowEngine.Persistence.Abstract.Tables;

namespace WorkflowEngine.Persistence.Memory.Tables
{
    public class WorkflowInstanceTableMemory : ManyToOneMemory<WorkflowInstanceRecordCreate, WorkflowInstanceRecord, Guid>, IWorkflowInstanceTable
    {
        /// <inheritdoc />
        public WorkflowInstanceTableMemory() : base(item => item.WorkflowVersionId)
        {
        }
    }
}