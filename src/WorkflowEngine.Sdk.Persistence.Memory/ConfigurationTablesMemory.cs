using System;
using System.Threading;
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

        /// <inheritdoc />
        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            // ActivityVersion
            var activityVersion = ActivityVersion as CrudMemory<ActivityVersionRecordCreate, ActivityVersionRecord, Guid>;
            FulcrumAssert.IsNotNull(activityVersion, CodeLocation.AsString());
            await activityVersion!.DeleteAllAsync(cancellationToken);

            // ActivityForm
            var activityForm = ActivityForm as CrudMemory<ActivityFormRecordCreate, ActivityFormRecord, Guid>;
            FulcrumAssert.IsNotNull(activityVersion, CodeLocation.AsString());
            await activityForm!.DeleteAllAsync(cancellationToken);

            // WorkflowVersion
            var workflowVersion = WorkflowVersion as CrudMemory<WorkflowVersionRecordCreate, WorkflowVersionRecord, Guid>;
            FulcrumAssert.IsNotNull(workflowVersion, CodeLocation.AsString());
            await workflowVersion!.DeleteAllAsync(cancellationToken);

            // WorkflowForm
            var workflowForm = WorkflowForm as CrudMemory<WorkflowFormRecordCreate, WorkflowFormRecord, Guid>;
            FulcrumAssert.IsNotNull(workflowVersion, CodeLocation.AsString());
            await workflowForm!.DeleteAllAsync(cancellationToken);
        }
    }
}
