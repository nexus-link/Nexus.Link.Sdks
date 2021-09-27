using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class ActivityVersionTableMemory : ManyToOneMemory<ActivityVersionRecordCreate, ActivityVersionRecord, Guid>, IActivityVersionTable
    {
        /// <inheritdoc />
        public ActivityVersionTableMemory() : base(item => item.WorkflowVersionId)
        {
        }
    }
}