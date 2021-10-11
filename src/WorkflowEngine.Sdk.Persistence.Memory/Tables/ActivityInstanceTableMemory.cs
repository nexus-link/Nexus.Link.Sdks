using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    public class ActivityInstanceTableMemory : CrudMemory<ActivityInstanceRecordCreate, ActivityInstanceRecord, Guid>, IActivityInstanceTable
    {
        public ActivityInstanceTableMemory()
        {
            UniqueConstraintMethods += item => new ActivityInstanceRecordUnique
            {
                WorkflowInstanceId = item.WorkflowInstanceId,
                ParentActivityInstanceId = item.ParentActivityInstanceId,
                ActivityVersionId = item.ActivityVersionId,
                ParentIteration = item.ParentIteration
            };
        }
    }
}