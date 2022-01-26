using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory
{
    public class RuntimeTablesMemory : IRuntimeTables
    {
        public RuntimeTablesMemory()
        {
            WorkflowInstance = new WorkflowInstanceTableMemory();
            ActivityInstance = new ActivityInstanceTableMemory();
            Log = new LogTableMemory();
        }

        /// <inheritdoc />
        public IWorkflowInstanceTable WorkflowInstance { get; }

        /// <inheritdoc />
        public IActivityInstanceTable ActivityInstance { get; }

        /// <inheritdoc />
        public ILogTable Log { get; }

        public async Task DeleteAllAsync()
        {
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
