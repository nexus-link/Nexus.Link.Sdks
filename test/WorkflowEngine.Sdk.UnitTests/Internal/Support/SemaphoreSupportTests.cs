using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.Internal.Activities;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Support
{
    public class SemaphoreSupportTests
    {
        public SemaphoreSupportTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivitySwitchTests));
        }

        [Fact]
        public async Task Raise_Given_SemaphoreRaisedWithSameParent_Throws_RequestPostponedException()
        {
            // Arrange
            var runtimeTablesMemory = new RuntimeTablesMemory();
            var activityExecutorMock = new Mock<IActivityExecutor>();
            var methodMock = new Mock<IMethodMock>();
            var workflowInformationMock = new WorkflowInformationMock(activityExecutorMock.Object)
            {
                SemaphoreService = new WorkflowSemaphoreService(null, runtimeTablesMemory),
                MethodMock = methodMock.Object
            };
            var resourceId = Guid.NewGuid().ToString();

            // Parent activity
            var parentActivityInformationMock = new ActivityInformationMock(workflowInformationMock);
            var parentActivityInstance =
                CreateActivity1Instance(parentActivityInformationMock.InstanceId, workflowInformationMock.InstanceId);
            methodMock.Setup(x => x.GetActivityInstance(parentActivityInformationMock.InstanceId)).Returns(parentActivityInstance);
            var parentActivity = new ActivityParallel(parentActivityInformationMock);

            parentActivity.InternalIteration = 1;
            // Activity 1
            var activityInformationMock1 = new ActivityInformationMock(workflowInformationMock)
            {
                InstanceId = "E7FC20D8-AB5C-41BA-AF5A-BCF6A6EF221B",
                Parent = parentActivity
            };
            var activity1Instance = CreateActivity1Instance(activityInformationMock1.InstanceId, workflowInformationMock.InstanceId);
            methodMock.Setup(x => x.GetActivityInstance(activityInformationMock1.InstanceId)).Returns(activity1Instance);
            var activity1 = new ActivitySemaphore(activityInformationMock1, resourceId);
            var semaphoreSupport1 = new SemaphoreSupport(resourceId, 1, null)
            {
                Activity = activity1
            };

            // Raise for activity 1
            await semaphoreSupport1.RaiseAsync();

            parentActivity.InternalIteration = 2;
            // Activity 2
            var activityInformationMock2 = new ActivityInformationMock(workflowInformationMock)
            {
                InstanceId = "76992EBF-89D3-49F5-9524-59F91ACAA228",
                Parent = parentActivity,
            };
            var activity2Instance = CreateActivity1Instance(activityInformationMock2.InstanceId, workflowInformationMock.InstanceId);
            methodMock.Setup(x => x.GetActivityInstance(activityInformationMock2.InstanceId)).Returns(activity2Instance);
            var activity2 = new ActivitySemaphore(activityInformationMock2, resourceId);
            var semaphoreSupport2 = new SemaphoreSupport(resourceId, 1, null)
            {
                Activity = activity2
            };

            // Act
            var exception = await semaphoreSupport2
                .RaiseAsync()
                .ShouldThrowAsync<RequestPostponedException>();

            // Assert
            exception.TryAgain.ShouldBe(false, "A semaphore should not be tried again after a time interval, but explicitly when the semaphore is lowered");
        }

        private static ActivityInstance CreateActivity1Instance(string id, string workflowInstanceId)
        {
            return new ActivityInstance
            {
                Id = id,
                WorkflowInstanceId = workflowInstanceId,
                StartedAt = DateTimeOffset.Now,
                ActivityVersionId = "D21EB356-C401-4A04-9469-12668DE28AE5",
                AsyncRequestId = null,
                ContextDictionary = new ConcurrentDictionary<string, JToken>(),
                Etag = "made up",
                State = ActivityStateEnum.Executing
            };
        }
    }
}

