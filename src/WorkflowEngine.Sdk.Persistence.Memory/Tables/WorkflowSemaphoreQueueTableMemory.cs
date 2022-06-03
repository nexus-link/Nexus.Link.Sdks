using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    /// <inheritdoc cref="IWorkflowSemaphoreQueueTable" />
    public class WorkflowSemaphoreQueueTableMemory : CrudMemory<WorkflowSemaphoreQueueRecordCreate, WorkflowSemaphoreQueueRecord, Guid>, IWorkflowSemaphoreQueueTable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowSemaphoreQueueTableMemory()
        {
            UniqueConstraintMethods += item => new WorkflowSemaphoreQueueRecordUnique
            {
                WorkflowSemaphoreId = item.WorkflowSemaphoreId,
                WorkflowInstanceId = item.WorkflowInstanceId,
                ParentActivityInstanceId = item.ParentActivityInstanceId,
                ParentIteration = item.ParentIteration
            };
        }
    }
}