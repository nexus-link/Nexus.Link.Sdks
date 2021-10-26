using System;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.UnitTests.Services
{
    public abstract class WorkflowServiceTestsBases
    {
        // System under test
        protected static readonly IWorkflowService WorkflowService;

        protected static WorkflowFormRecord WorkflowFormRecord;
        protected static WorkflowVersionRecord WorkflowVersionRecord;
        protected static WorkflowInstanceRecord WorkflowInstanceRecord;
        protected static ActivityInstanceRecord GrandChildActivity;
        
        protected readonly ITestOutputHelper TestOutputHelper;
        protected static readonly IConfigurationTables ConfigurationTables = new ConfigurationTablesMemory();
        protected static readonly IRuntimeTables RuntimeTables = new RuntimeTablesMemory();

        static WorkflowServiceTestsBases()
        {
            var asyncRequestMgmtCapabilityMock = new Mock<IAsyncRequestMgmtCapability>();
            WorkflowService = new WorkflowService(ConfigurationTables, RuntimeTables, asyncRequestMgmtCapabilityMock.Object);

            SetupWorkflowMockStructureAsync().Wait();
        }

        protected WorkflowServiceTestsBases(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }

        private static async Task SetupWorkflowMockStructureAsync()
        {
            WorkflowFormRecord = await ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), new WorkflowFormRecordCreate
            {
                CapabilityName = "CAP",
                Title = "FORM ALDEHYDE"
            });
            var versionId = await ConfigurationTables.WorkflowVersion.CreateAsync(new WorkflowVersionRecordCreate
            {
                WorkflowFormId = WorkflowFormRecord.Id,
                MajorVersion = 1,
                MinorVersion = 0
            });
            WorkflowVersionRecord = await ConfigurationTables.WorkflowVersion.ReadAsync(versionId);
            WorkflowInstanceRecord = await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), new WorkflowInstanceRecordCreate
            {
                Title = "INSTANCE TITLE",
                WorkflowVersionId = WorkflowVersionRecord.Id,
                StartedAt = DateTimeOffset.Now,
                InitialVersion = "1.0"
            });

            var (formIdA, versionIdA, instanceIdA) = await CreateActivityTrio("A top", 1);
            var (formIdAc, versionIdAc, instanceIdAc) = await CreateActivityTrio("A child", 1, versionIdA, instanceIdA);
            var (formIdAgc, versionIdAgc, instanceIdgc) = await CreateActivityTrio("A grand child", 1, versionIdAc, instanceIdAc);
            GrandChildActivity = await RuntimeTables.ActivityInstance.ReadAsync(instanceIdgc);

            var (formIdB, versionIdB, instanceIdB) = await CreateActivityTrio("B top", 2);
            var (formIdBc, versionIdBc, instanceIdBc) = await CreateActivityTrio("B child", 1, versionIdB, instanceIdB);
        }

        private static async Task<(Guid formId, Guid versionId, Guid instanceId)> CreateActivityTrio(string title, int position, Guid? parentActivityVersionId = null, Guid? parentActivityInstanceId = null)
        {
            var activityForm = await ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), new ActivityFormRecordCreate
            {
                Title = title,
                WorkflowFormId = WorkflowFormRecord.Id,
                Type = ActivityTypeEnum.Action.ToString()
            });
            var activityVersionId = await ConfigurationTables.ActivityVersion.CreateAsync(new ActivityVersionRecordCreate
            {
                ActivityFormId = activityForm.Id,
                WorkflowVersionId = WorkflowVersionRecord.Id,
                Position = position,
                FailUrgency = ActivityFailUrgencyEnum.Stopping.ToString(),
                ParentActivityVersionId = parentActivityVersionId
            });
            var activityInstance = await RuntimeTables.ActivityInstance.CreateAndReturnAsync(new ActivityInstanceRecordCreate
            {
                ActivityVersionId = activityVersionId,
                StartedAt = DateTimeOffset.Now,
                WorkflowInstanceId = WorkflowInstanceRecord.Id,
                ParentActivityInstanceId = parentActivityInstanceId,
                State = ActivityStateEnum.Started.ToString()
            });
            return (activityForm.Id, activityVersionId, activityInstance.Id);
        }
    }
}
