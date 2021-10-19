using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Moq;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.WorkflowLogic;
using Shouldly;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
{
    public class ActivityExecutorTests
    {
        private readonly Mock<IAsyncRequestClient> _asyncRequestClientMock;
        private readonly ActivityInformation _activityInformation;
        private readonly IRuntimeTables _runtimeTables;

        public ActivityExecutorTests()
        {
            var configurationTables = new ConfigurationTablesMemory();
            _runtimeTables = new RuntimeTablesMemory();
            var workflowCapability = new WorkflowCapability(configurationTables, _runtimeTables, null);
            var workflowInformation = new WorkflowInformation(workflowCapability, new MethodHandler("Workflow"))
            {
                FormId = "CD72BDE7-4D6A-42A6-B683-28CFB2AFD122",
                VersionId = "C5739B52-CAEF-4EAE-BEFB-61F01C54501A",
                InstanceId = "F2746172-ADD3-49CF-94B8-548536DD578D"
            };
            _asyncRequestClientMock = new Mock<IAsyncRequestClient>();
            _activityInformation = new ActivityInformation(workflowInformation, new MethodHandler("Activity"), 1,
                WorkflowActivityTypeEnum.Action, null, null)
            {
                FormId = "0D759290-9F93-4B3A-8333-76019DE227CF",
                FormTitle = "Form title",
                State = ActivityStateEnum.Started
            };
        }

        [Fact]
        public async Task Execute_Given_MethodReturns_Gives_Success()
        {
            // Arrange
            var executor = new ActivityExecutor(_asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityInformation, executor, null, null);
            const int expectedValue = 10;

            // Act
            var actualValue = await executor.ExecuteAsync((a, t) => Task.FromResult(expectedValue), null);

            // Assert
            actualValue.ShouldBe(expectedValue);
            _activityInformation.InstanceId.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityInformation.InstanceId));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Success.ToString());
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsAndStopping_Gives_Postponed()
        {
            // Arrange
            var executor = new ActivityExecutor(_asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityInformation, executor, null, null);
            _activityInformation.FailUrgency = ActivityFailUrgencyEnum.Stopping;
            const int expectedValue = 10;

            // Act & Assert
            RequestPostponedException postponed = null;
            try
            {
                await executor.ExecuteAsync<int>(
                    (a, t) => throw new Exception("Fail"), null);
            }
            catch (Exception e)
            {
                e.ShouldBeAssignableTo<RequestPostponedException>();
                postponed = e as RequestPostponedException;
            }
            postponed.ShouldNotBeNull();
            _activityInformation.InstanceId.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityInformation.InstanceId));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
        }

        [Theory]
        [InlineData(ActivityFailUrgencyEnum.HandleLater)]
        [InlineData(ActivityFailUrgencyEnum.Ignore)]
        public async Task Execute_Given_MethodThrowsAndNotStopping_Gives_Default(ActivityFailUrgencyEnum failUrgency)
        {
            // Arrange
            var executor = new ActivityExecutor(_asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityInformation, executor, null, null);
            _activityInformation.FailUrgency = failUrgency;
            const int expectedValue = 10;

            // Act
            var actualValue = await executor.ExecuteAsync(
                (a, t) => throw new Exception("Fail"), ct => Task.FromResult(expectedValue));
            _activityInformation.InstanceId.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityInformation.InstanceId));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
            actualValue.ShouldBe(expectedValue);
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsRequestPostponed_Gives_RequestIdSet()
        {
            // Arrange
            var executor = new ActivityExecutor(_asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityInformation, executor, null, null);
            var expectedRequestId = Guid.NewGuid().ToString();

            // Act & Assert
            await Assert.ThrowsAnyAsync<RequestPostponedException>(
                () => executor.ExecuteAsync<int>(
                    (a, t) => throw new RequestPostponedException(expectedRequestId), null));
            _activityInformation.InstanceId.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityInformation.InstanceId));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Waiting.ToString());
            instance.AsyncRequestId.ShouldBe(expectedRequestId);
        }

        [Fact]
        public async Task Execute_Given_HasRequestIdButNotReady_Gives_Postpone()
        {
            // Arrange
            var executor = new ActivityExecutor(_asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityInformation, executor, null, null);
            var expectedRequestId = Guid.NewGuid().ToString();
            await Assert.ThrowsAnyAsync<RequestPostponedException>(
                () => executor.ExecuteAsync<int>(
                    (a, t) => throw new RequestPostponedException(expectedRequestId), null));
            executor = new ActivityExecutor(_asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityInformation, executor, null, null);

            // Act & Assert
            RequestPostponedException postponed = null;
            try
            {
                await executor.ExecuteAsync<int>(
                    (a, t) => Task.FromResult(10), null);
            }
            catch (Exception e)
            {
                e.ShouldBeAssignableTo<RequestPostponedException>();
                postponed = e as RequestPostponedException;
            }
            postponed.ShouldNotBeNull();
            postponed.WaitingForRequestIds.ShouldContain(expectedRequestId);
            _activityInformation.InstanceId.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityInformation.InstanceId));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Waiting.ToString());
            instance.AsyncRequestId.ShouldBe(expectedRequestId);
        }
    }
}
