using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class ActivityVersionTableMemory : CrudMemory<ActivityVersionRecordCreate, ActivityVersionRecord, Guid>, IActivityVersionTable
    {
        public ActivityVersionTableMemory()
        {
            UniqueConstraintMethods += item => new ActivityVersionRecordUnique
            {
                WorkflowVersionId = item.WorkflowVersionId,
                ActivityFormId = item.ActivityFormId
            };
        }
    }
}