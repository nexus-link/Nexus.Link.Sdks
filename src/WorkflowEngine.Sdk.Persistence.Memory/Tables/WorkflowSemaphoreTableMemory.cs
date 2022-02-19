using System;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables
{
    /// <inheritdoc cref="IWorkflowSemaphoreTable" />
    public class WorkflowSemaphoreTableMemory : CrudMemory<WorkflowSemaphoreRecordCreate, WorkflowSemaphoreRecord, Guid>, IWorkflowSemaphoreTable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowSemaphoreTableMemory()
        {
            UniqueConstraintMethods += item => new WorkflowSemaphoreRecordUnique
            {
                WorkflowFormId = item.WorkflowFormId,
                ResourceIdentifier = item.ResourceIdentifier
            };
        }
    }
}