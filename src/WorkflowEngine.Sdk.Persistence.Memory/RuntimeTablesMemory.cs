using System;
using System.Threading.Tasks;
using Nexus.Link.Contracts.Misc.Sdk.Authentication;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory
{
    /// <inheritdoc />
    public class RuntimeTablesMemory : IRuntimeTables
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RuntimeTablesMemory()
        {
            WorkflowInstance = new WorkflowInstanceTableMemory();
            ActivityInstance = new ActivityInstanceTableMemory();
            Log = new LogTableMemory();
            WorkflowSemaphore = new WorkflowSemaphoreTableMemory();
            WorkflowSemaphoreQueue = new WorkflowSemaphoreQueueTableMemory();
            Hash = new HashTableMemory();
        }

        /// <inheritdoc />
        public IWorkflowInstanceTable WorkflowInstance { get; }

        /// <inheritdoc />
        public IActivityInstanceTable ActivityInstance { get; }

        /// <inheritdoc />
        public ILogTable Log { get; }

        /// <inheritdoc />
        public IWorkflowSemaphoreTable WorkflowSemaphore { get; }

        /// <inheritdoc />
        public IWorkflowSemaphoreQueueTable WorkflowSemaphoreQueue { get; }

        /// <inheritdoc />
        public IHashTable Hash { get; }

        /// <summary>
        /// Use this for testing purposes to delete all runtime table records.
        /// </summary>
        public async Task DeleteAllAsync()
        {
            // Log
            var log = Log as CrudMemory<LogRecordCreate, LogRecord, Guid>;
            FulcrumAssert.IsNotNull(log, CodeLocation.AsString());
            await log!.DeleteAllAsync();

            // WorkflowSemaphore
            var workflowSemaphore = WorkflowSemaphore as CrudMemory<WorkflowSemaphoreRecordCreate, WorkflowSemaphoreRecord, Guid>;
            FulcrumAssert.IsNotNull(workflowSemaphore, CodeLocation.AsString());
            await workflowSemaphore!.DeleteAllAsync();

            // ActivityInstance
            var activityInstance = ActivityInstance as CrudMemory<ActivityInstanceRecordCreate, ActivityInstanceRecord, Guid>;
            FulcrumAssert.IsNotNull(activityInstance, CodeLocation.AsString());
            await activityInstance!.DeleteAllAsync();

            // WorkflowInstance
            var workflowInstance = WorkflowInstance as CrudMemory<WorkflowInstanceRecordCreate, WorkflowInstanceRecord, Guid>;
            FulcrumAssert.IsNotNull(workflowInstance, CodeLocation.AsString());
            await workflowInstance!.DeleteAllAsync();
        }
    }
}
