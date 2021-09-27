using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class ActivityFormTableMemory : ManyToOneMemory<ActivityFormRecordCreate, ActivityFormRecord, Guid>, IActivityFormTable
    {
        /// <inheritdoc />
        public ActivityFormTableMemory() : base(item => item.WorkflowFormId)
        {
        }
    }
}