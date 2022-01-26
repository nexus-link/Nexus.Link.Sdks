using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory
{
    public class ConfigurationTablesMemory : IConfigurationTables
    {
        public ConfigurationTablesMemory()
        {
            WorkflowForm = new WorkflowFormTableMemory();
            WorkflowVersion = new WorkflowVersionTableMemory();
            ActivityForm = new ActivityFormTableMemory();
            ActivityVersion = new ActivityVersionTableMemory();
        }
        /// <inheritdoc />
        public IWorkflowFormTable WorkflowForm { get; }

        /// <inheritdoc />
        public IWorkflowVersionTable WorkflowVersion { get; }

        /// <inheritdoc />
        public IActivityFormTable ActivityForm { get; }

        /// <inheritdoc />
        public IActivityVersionTable ActivityVersion { get; }

        public async Task DeleteAllAsync()
        {
            // ActivityVersion
            var activityVersion = ActivityVersion as CrudMemory<ActivityVersionRecordCreate, ActivityVersionRecord, Guid>;
            FulcrumAssert.IsNotNull(activityVersion, CodeLocation.AsString());
            await activityVersion!.DeleteAllAsync();

            // ActivityForm
            var activityForm = ActivityForm as CrudMemory<ActivityFormRecordCreate, ActivityFormRecord, Guid>;
            FulcrumAssert.IsNotNull(activityVersion, CodeLocation.AsString());
            await activityForm!.DeleteAllAsync();

            // WorkflowVersion
            var workflowVersion = WorkflowVersion as CrudMemory<WorkflowVersionRecordCreate, WorkflowVersionRecord, Guid>;
            FulcrumAssert.IsNotNull(workflowVersion, CodeLocation.AsString());
            await workflowVersion!.DeleteAllAsync();

            // WorkflowForm
            var workflowForm = WorkflowForm as CrudMemory<WorkflowFormRecordCreate, WorkflowFormRecord, Guid>;
            FulcrumAssert.IsNotNull(workflowVersion, CodeLocation.AsString());
            await workflowForm!.DeleteAllAsync();
        }
    }
}
