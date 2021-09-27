using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using WorkflowEngine.Persistence.Abstract.Entities;
using WorkflowEngine.Persistence.Abstract.Tables;

namespace WorkflowEngine.Persistence.Memory.Tables
{
    public class ActivityFormTableMemory : ManyToOneMemory<ActivityFormRecordCreate, ActivityFormRecord, Guid>, IActivityFormTable
    {
        /// <inheritdoc />
        public ActivityFormTableMemory() : base(item => item.WorkflowFormId)
        {
        }
    }
}