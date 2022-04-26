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
using Nexus.Link.WorkflowEngine.Sdk.Support;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
{
    public class ActivityExecutorTests
    {
        private readonly AsyncRequestMgmtMock _asyncRequestMgmtCapabilityMock;
        private readonly IRuntimeTables _runtimeTables;
        private readonly WorkflowCache _workflowCache;
        private readonly IWorkflowImplementationBase _workflowImplementation;
        private readonly IConfigurationTables _configurationTables;
        private readonly ActivityInformationMock _activityInformation;

        public ActivityExecutorTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityExecutorTests));
            _configurationTables = new ConfigurationTablesMemory();
            _runtimeTables = new RuntimeTablesMemory();

            var asyncRequestMgmtCapabilityMock = new Mock<IAsyncRequestMgmtCapability>();
            _asyncRequestMgmtCapabilityMock = new AsyncRequestMgmtMock();
            var workflowCapabilities = new WorkflowCapabilities(_configurationTables, _runtimeTables, _asyncRequestMgmtCapabilityMock);
            _workflowImplementation = new TestWorkflowImplementation(workflowCapabilities);
            var workflowInformation = new WorkflowInformation(_workflowImplementation);
            workflowInformation.InstanceId = Guid.NewGuid().ToGuidString();

            _workflowCache = new WorkflowCache(workflowInformation, workflowCapabilities);
            _workflowCache.LoadAsync(default).Wait();
            _activityInformation = new ActivityInformationMock();
        }

        [Fact]
        public async Task Execute_Given_MethodReturns_Gives_Success()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityInformation, null);
            var executor = new ActivityExecutor(activity);
            const int expectedValue = 10;
            var minTime = DateTimeOffset.UtcNow;

            // Act
            var actualValue = await executor.ExecuteWithReturnValueAsync(_ => Task.FromResult(expectedValue), null);

            // Assert
            actualValue.ShouldBe(expectedValue);
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsAndStopping_Gives_Postponed()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityInformation, null);
            var executor = new ActivityExecutor(activity);
            executor.Activity.Version.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            RequestPostponedException postponed = null;
            try
            {
                await executor.ExecuteWithoutReturnValueAsync(_ => throw new Exception("Fail"));
            }
            catch (Exception e)
            {
                e.ShouldBeAssignableTo<WorkflowImplementationShouldNotCatchThisException>();
                e.InnerException.ShouldBeAssignableTo<RequestPostponedException>();
                postponed = e.InnerException as RequestPostponedException;
            }
            postponed.ShouldNotBeNull();
            executor.Activity.Instance.Id.ShouldNotBeNull();
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsAndStopping_Gives_AlertHandlerCalled()
        {
            // Arrange
            bool? alertResult = null;
            var workflowCapabilities = new WorkflowCapabilities(_configurationTables, _runtimeTables, _asyncRequestMgmtCapabilityMock);
            _activityInformation.Options = new ActivityOptions
            {
                ExceptionAlertHandler = (alert, token) =>
                {
                    alertResult = true;
                    return Task.FromResult(true);
                }
            };
            var activity = new ActivityAction<int>(_activityInformation, null);
            var executor = new ActivityExecutor(activity);
            executor.Activity.Version.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            await Should.ThrowAsync<WorkflowImplementationShouldNotCatchThisException>(() => executor.ExecuteWithoutReturnValueAsync(
                   _ =>throw new Exception("Fail")));

            alertResult.ShouldBe(true);
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = executor.Activity.Instance;
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed);
            instance.ExceptionAlertHandled.ShouldBe(true);
        }

        [Theory]
        [InlineData(ActivityFailUrgencyEnum.HandleLater)]
        [InlineData(ActivityFailUrgencyEnum.Ignore)]
        public async Task Execute_Given_MethodThrowsAndNotStopping_Gives_Default(ActivityFailUrgencyEnum failUrgency)
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityInformation, null);
            var executor = new ActivityExecutor(activity);
            executor.Activity.Version.FailUrgency = failUrgency;
            const int expectedValue = 10;

            // Act
            var actualValue = await executor.ExecuteWithReturnValueAsync(
                _ =>throw new Exception("Fail"), ct => Task.FromResult(expectedValue));
            await _workflowCache.SaveAsync();
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = executor.Activity.Instance;
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed);
            actualValue.ShouldBe(expectedValue);
            instance.ExceptionCategory.ShouldNotBeNull();
            instance.ExceptionFriendlyMessage.ShouldNotBeNullOrWhiteSpace();
            instance.ExceptionTechnicalMessage.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsRequestPostponed_Gives_RequestIdSet()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityInformation, null);
            var executor = new ActivityExecutor(activity);
            var expectedRequestId = Guid.NewGuid().ToGuidString();

            // Act & Assert
            await Should.ThrowAsync<WorkflowImplementationShouldNotCatchThisException>(
                () => executor.ExecuteWithReturnValueAsync<int>(
                    _ =>throw new RequestPostponedException(expectedRequestId), null));
            await _workflowCache.SaveAsync();
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = executor.Activity.Instance;
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Waiting);
            instance.AsyncRequestId.ShouldBe(expectedRequestId);
        }

        [Fact]
        public async Task Execute_Given_FulcrumTryAgainException_Gives_PostponeTryAgain()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityInformation, null);
            var executor = new ActivityExecutor(activity);
            executor.Activity.Version.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            FulcrumTryAgainException tryAgain = null;
            try
            {
                await executor.ExecuteWithReturnValueAsync<int>(
                    _ =>throw new FulcrumTryAgainException("Fail"), null);
            }
            catch (Exception e)
            {
                e.ShouldBeAssignableTo<WorkflowImplementationShouldNotCatchThisException>();
                e.InnerException.ShouldBeAssignableTo<FulcrumTryAgainException>();
                tryAgain = e.InnerException as FulcrumTryAgainException;
            }
            await _workflowCache.SaveAsync();
            tryAgain.ShouldNotBeNull();
            executor.Activity.Instance.Id.ShouldNotBeNull();
            var instance = executor.Activity.Instance;
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Waiting);
        }
    }
}

