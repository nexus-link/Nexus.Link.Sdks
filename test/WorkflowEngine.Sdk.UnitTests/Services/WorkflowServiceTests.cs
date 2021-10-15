using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.UnitTests.Services
{
    public class WorkflowServiceTests
    {
        // System under test
        private readonly IWorkflowService _workflowService;

        #region Mock setup
        private readonly Mock<IActivityFormTable> _activityFormTableMock = new Mock<IActivityFormTable>();
        private readonly Mock<IActivityVersionTable> _activityVersionTableMock = new Mock<IActivityVersionTable>();
        private readonly Mock<IWorkflowFormTable> _workflowFormTableMock = new Mock<IWorkflowFormTable>();
        private readonly Mock<IWorkflowVersionTable> _workflowVersionTableMock = new Mock<IWorkflowVersionTable>();

        private readonly Mock<IActivityInstanceTable> _activityInstanceTableMock = new Mock<IActivityInstanceTable>();
        private readonly Mock<IWorkflowInstanceTable> _workflowInstanceTableMock = new Mock<IWorkflowInstanceTable>();
        #endregion

        #region Data setup

        private static readonly WorkflowFormRecord WorkflowFormRecord = new WorkflowFormRecord { Id = Guid.NewGuid(), CapabilityName = "CAP", Title = "FORM ALDEHYDE" };
        private static readonly WorkflowVersionRecord WorkflowVersionRecord = new WorkflowVersionRecord { Id = Guid.NewGuid(), WorkflowFormId = WorkflowFormRecord.Id, MajorVersion = 1, MinorVersion = 0 };
        private static readonly WorkflowInstanceRecord WorkflowInstanceRecord = new WorkflowInstanceRecord { Id = Guid.NewGuid(), Title = "INSTANCE TITLE", WorkflowVersionId = WorkflowVersionRecord.Id, StartedAt = DateTimeOffset.Now, InitialVersion = "1.0" };

        private static readonly List<ActivityFormRecord> ActivityForms = new List<ActivityFormRecord>
        {
            new ActivityFormRecord { Id = Guid.NewGuid(), Title = "A", WorkflowFormId = WorkflowFormRecord.Id, Type = "x" },
            new ActivityFormRecord { Id = Guid.NewGuid(), Title = "B", WorkflowFormId = WorkflowFormRecord.Id, Type = "x" },
        };

        private static int _topLevelPosition = 1;
        private static readonly List<ActivityVersionRecord> ActivityVersions = ActivityForms.Select(x => new ActivityVersionRecord
        {
            Id = Guid.NewGuid(),
            ActivityFormId = x.Id,
            WorkflowVersionId = WorkflowVersionRecord.Id,
            Position = _topLevelPosition++
        }).ToList();

        private static readonly List<ActivityInstanceRecord> ActivityInstances = ActivityVersions.Select(x => new ActivityInstanceRecord
        {
            Id = Guid.NewGuid(),
            ActivityVersionId = x.Id,
            StartedAt = DateTimeOffset.Now,
            WorkflowInstanceId = WorkflowInstanceRecord.Id,
        }).ToList();

        private static int _childLevelPosition = 1;
        private static readonly List<ActivityVersionRecord> ActivityVersionsChildren = ActivityVersions.Select(x => new ActivityVersionRecord
        {
            Id = Guid.NewGuid(),
            ActivityFormId = x.ActivityFormId,
            ParentActivityVersionId = x.Id,
            WorkflowVersionId = x.WorkflowVersionId,
            Position = _childLevelPosition++
        }).ToList();

        private static readonly List<ActivityInstanceRecord> ActivityInstancesChildren = ActivityInstances.Select(x => new ActivityInstanceRecord
        {
            Id = Guid.NewGuid(),
            ActivityVersionId = x.ActivityVersionId,
            ParentActivityInstanceId = x.Id,
            StartedAt = DateTimeOffset.Now,
            WorkflowInstanceId = x.WorkflowInstanceId
        }).ToList();

        #endregion

        private readonly ITestOutputHelper _testOutputHelper;

        public WorkflowServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            var configurationTables = new Mock<IConfigurationTables>();
            configurationTables.Setup(x => x.ActivityForm).Returns(_activityFormTableMock.Object);
            configurationTables.Setup(x => x.ActivityVersion).Returns(_activityVersionTableMock.Object);
            configurationTables.Setup(x => x.WorkflowForm).Returns(_workflowFormTableMock.Object);
            configurationTables.Setup(x => x.WorkflowVersion).Returns(_workflowVersionTableMock.Object);

            var runtimeTables = new Mock<IRuntimeTables>();
            runtimeTables.Setup(x => x.ActivityInstance).Returns(_activityInstanceTableMock.Object);
            runtimeTables.Setup(x => x.WorkflowInstance).Returns(_workflowInstanceTableMock.Object);

            _workflowService = new WorkflowService(configurationTables.Object, runtimeTables.Object);

            SetupWorkflowMockStructure();
        }

        private void SetupWorkflowMockStructure()
        {
            var eTagItems = new List<object> { WorkflowFormRecord, WorkflowVersionRecord, WorkflowInstanceRecord };
            eTagItems.AddRange(ActivityForms);
            eTagItems.AddRange(ActivityVersions);
            eTagItems.AddRange(ActivityInstances);
            eTagItems.AddRange(ActivityVersionsChildren);
            eTagItems.AddRange(ActivityInstancesChildren);
            foreach (var record in eTagItems)
            {
                ((ICompleteTableItem)record).Etag = Guid.NewGuid().ToString();
            }

            _workflowFormTableMock.Setup(x => x.ReadAsync(WorkflowFormRecord.Id, It.IsAny<CancellationToken>())).ReturnsAsync(WorkflowFormRecord);
            _workflowVersionTableMock.Setup(x => x.ReadAsync(WorkflowVersionRecord.Id, It.IsAny<CancellationToken>())).ReturnsAsync(WorkflowVersionRecord);
            _workflowInstanceTableMock.Setup(x => x.ReadAsync(WorkflowInstanceRecord.Id, It.IsAny<CancellationToken>())).ReturnsAsync(WorkflowInstanceRecord);

            _activityFormTableMock
                .Setup(x => x.SearchAsync(It.IsAny<SearchDetails<ActivityFormRecord>>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PageEnvelope<ActivityFormRecord> { Data = ActivityForms });

            _activityVersionTableMock
                .Setup(x => x.SearchAsync(It.IsAny<SearchDetails<ActivityVersionRecord>>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PageEnvelope<ActivityVersionRecord> { Data = ActivityVersions.Concat(ActivityVersionsChildren) });
            _activityInstanceTableMock
                .Setup(x => x.SearchAsync(It.IsAny<SearchDetails<ActivityInstanceRecord>>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PageEnvelope<ActivityInstanceRecord> { Data = ActivityInstances.Concat(ActivityInstancesChildren) });
        }

        [Fact]
        public async Task The_Convenience_Workflow_Contains_All_Information_And_Hierarchy()
        {
            // Arrange
            var id = WorkflowInstanceRecord.Id.ToString();

            // Act
            var workflow = await _workflowService.ReadAsync(id);
            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(workflow, Formatting.Indented));

            // Assert
            workflow.Form.Id.ShouldBe(WorkflowFormRecord.Id.ToString());
            workflow.Version.Id.ShouldBe(WorkflowVersionRecord.Id.ToString());
            workflow.Instance.Id.ShouldBe(id);
            workflow.Activities.Count.ShouldBe(ActivityInstances.Count);
            foreach (var activity in workflow.Activities)
            {
                activity.Instance.WorkflowInstanceId.ShouldBe(WorkflowInstanceRecord.Id.ToString());
                activity.Children.Count.ShouldBe(1);
                activity.Children[0].Instance.ParentActivityInstanceId.ShouldBe(activity.Instance.Id);
            }
        }
    }
}
