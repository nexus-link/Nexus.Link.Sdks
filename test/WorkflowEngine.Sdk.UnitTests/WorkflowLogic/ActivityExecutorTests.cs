using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Model;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
{
    public class ActivityExecutorTests
    {
        private readonly AsyncRequestMgmtMock _asyncRequestMgmtCapabilityMock;
        private readonly IRuntimeTables _runtimeTables;
        private readonly Mock<IWorkflowImplementation> _workflowVersionMock;
        private readonly IInternalActivityFlow _activityFlowMock;
        private readonly WorkflowCache _workflowCache;
        private readonly IWorkflowImplementationBase _workflowImplementation;
        private readonly IConfigurationTables _configurationTables;

        public ActivityExecutorTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityExecutorTests));
            _configurationTables = new ConfigurationTablesMemory();
            _runtimeTables = new RuntimeTablesMemory();

            var asyncRequestMgmtCapabilityMock = new Mock<IAsyncRequestMgmtCapability>();
            _asyncRequestMgmtCapabilityMock = new AsyncRequestMgmtMock();
            var workflowCapabilities = new WorkflowCapabilities(_configurationTables, _runtimeTables, _asyncRequestMgmtCapabilityMock);
            _workflowImplementation = new TestWorkflowImplementation(workflowCapabilities);
            var workflowInfo = new WorkflowInformation(_workflowImplementation)
            {
                InstanceId = Guid.NewGuid().ToGuidString()
            };
            _workflowCache = new WorkflowCache(workflowInfo);
            _workflowCache.LoadAsync(default).Wait();
            _workflowVersionMock = new Mock<IWorkflowImplementation>();
            _activityFlowMock = new ActivityFlowMock(workflowInfo,
                _workflowCache, "Form title", "0D759290-9F93-4B3A-8333-76019DE227CF", 1);
        }

        [Fact]
        public async Task Execute_Given_MethodReturns_Gives_Success()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityFlowMock, null);
            var executor = new ActivityExecutor(_workflowImplementation, activity);
            const int expectedValue = 10;
            var minTime = DateTimeOffset.UtcNow;

            // Act
            var actualValue = await executor.ExecuteAsync((a, t) => Task.FromResult(expectedValue), null);
            await _workflowCache.SaveAsync();

            // Assert
            actualValue.ShouldBe(expectedValue);
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(executor.Activity.Instance.Id.ToGuid());
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Success.ToString());
            var maxTime = DateTimeOffset.UtcNow;
            instance.FinishedAt.ShouldNotBeNull();
            instance.FinishedAt.Value.ShouldBeInRange(minTime, maxTime);
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsAndStopping_Gives_Postponed()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityFlowMock, null);
            var executor = new ActivityExecutor(_workflowImplementation, activity);
            executor.Activity.Version.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            RequestPostponedException postponed = null;
            try
            {
                await executor.ExecuteAsync(
                    (a, t) => throw new Exception("Fail"));
            }
            catch (Exception e)
            {
                e.ShouldBeAssignableTo<ExceptionTransporter>();
                e.InnerException.ShouldBeAssignableTo<RequestPostponedException>();
                postponed = e.InnerException as RequestPostponedException;
            }
            await _workflowCache.SaveAsync();
            postponed.ShouldNotBeNull();
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(executor.Activity.Instance.Id.ToGuid());
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsAndStopping_Gives_AlertHandlerCalled()
        {
            // Arrange
            bool? alertResult = null;
            var workflowCapabilities = new WorkflowCapabilities(_configurationTables, _runtimeTables, _asyncRequestMgmtCapabilityMock);
            var implementation = new TestWorkflowImplementation(workflowCapabilities);
            var workflowInformation = new WorkflowInformation(implementation)
            {
                DefaultActivityOptions =
                {
                    ExceptionAlertHandler = (alert, token) =>
                    {
                        alertResult = true;
                        return Task.FromResult(true);
                    }
                }
            };
            var activityFlowMock = new ActivityFlowMock(workflowInformation,
                _workflowCache, "Form title", "0D759290-9F93-4B3A-8333-76019DE227CF", 1);
            var activity = new ActivityAction<int>(activityFlowMock, null);
            var executor = new ActivityExecutor(implementation, activity);
            executor.Activity.Version.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            await Should.ThrowAsync<ExceptionTransporter>(() => executor.ExecuteAsync(
                   (a, t) => throw new Exception("Fail")));
            await _workflowCache.SaveAsync();

            alertResult.ShouldBe(true);
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(executor.Activity.Instance.Id.ToGuid());
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
            instance.ExceptionAlertHandled.ShouldBe(true);
        }

        [DataTestMethod]
        [DataRow(ActivityFailUrgencyEnum.HandleLater)]
        [DataRow(ActivityFailUrgencyEnum.Ignore)]
        public async Task Execute_Given_MethodThrowsAndNotStopping_Gives_Default(ActivityFailUrgencyEnum failUrgency)
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityFlowMock, null);
            var executor = new ActivityExecutor(_workflowImplementation, activity);
            executor.Activity.Version.FailUrgency = failUrgency;
            const int expectedValue = 10;

            // Act
            var actualValue = await executor.ExecuteAsync(
                (a, t) => throw new Exception("Fail"), ct => Task.FromResult(expectedValue));
            await _workflowCache.SaveAsync();
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(executor.Activity.Instance.Id.ToGuid());
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
            actualValue.ShouldBe(expectedValue);
            instance.ExceptionCategory.ShouldNotBeNullOrWhiteSpace();
            instance.ExceptionFriendlyMessage.ShouldNotBeNullOrWhiteSpace();
            instance.ExceptionTechnicalMessage.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsRequestPostponed_Gives_RequestIdSet()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityFlowMock, null);
            var executor = new ActivityExecutor(_workflowImplementation, activity);
            var expectedRequestId = Guid.NewGuid().ToGuidString();

            // Act & Assert
            await Should.ThrowAsync<ExceptionTransporter>(
                () => executor.ExecuteAsync<int>(
                    (a, t) => throw new RequestPostponedException(expectedRequestId), null));
            await _workflowCache.SaveAsync();
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(executor.Activity.Instance.Id.ToGuid());
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Waiting.ToString());
            instance.AsyncRequestId.ShouldBe(expectedRequestId);
        }

        [Fact]
        public async Task Execute_Given_FulcrumTryAgainException_Gives_PostponeTryAgain()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityFlowMock, null);
            var executor = new ActivityExecutor(_workflowImplementation, activity);
            executor.Activity.Version.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            FulcrumTryAgainException tryAgain = null;
            try
            {
                await executor.ExecuteAsync<int>(
                    (a, t) => throw new FulcrumTryAgainException("Fail"), null);
            }
            catch (Exception e)
            {
                e.ShouldBeAssignableTo<ExceptionTransporter>();
                e.InnerException.ShouldBeAssignableTo<FulcrumTryAgainException>();
                tryAgain = e.InnerException as FulcrumTryAgainException;
            }
            await _workflowCache.SaveAsync();
            tryAgain.ShouldNotBeNull();
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(executor.Activity.Instance.Id.ToGuid());
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Waiting.ToString());
        }
    }
}

