using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Moq;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
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
        private readonly ActivityPersistence _activityPersistence;
        private readonly IRuntimeTables _runtimeTables;
        private readonly Mock<IWorkflowVersionBase> _workflowVersionMock;

        public ActivityExecutorTests()
        {
            var configurationTables = new ConfigurationTablesMemory();
            _runtimeTables = new RuntimeTablesMemory();
            
            var asyncRequestMgmtCapabilityMock = new Mock<IAsyncRequestMgmtCapability>();
            var workflowCapability = new WorkflowCapability(configurationTables, _runtimeTables, asyncRequestMgmtCapabilityMock.Object);
            var workflowInformation = new WorkflowPersistence(workflowCapability, new MethodHandler("Workflow"))
            {
                FormId = "CD72BDE7-4D6A-42A6-B683-28CFB2AFD122",
                VersionId = "C5739B52-CAEF-4EAE-BEFB-61F01C54501A",
                InstanceId = "F2746172-ADD3-49CF-94B8-548536DD578D"
            };
            _workflowVersionMock = new Mock<IWorkflowVersionBase>();
            _asyncRequestClientMock = new Mock<IAsyncRequestClient>();
            _activityPersistence = new ActivityPersistence(workflowInformation, new MethodHandler("Activity"),
                "Form title", 1, "0D759290-9F93-4B3A-8333-76019DE227CF", ActivityTypeEnum.Action);
        }

        [Fact]
        public async Task Execute_Given_MethodReturns_Gives_Success()
        {
            // Arrange
            var executor = new ActivityExecutor(_workflowVersionMock.Object, _asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityPersistence, executor, null);
            const int expectedValue = 10;
            var minTime = DateTimeOffset.UtcNow;

            // Act
            var actualValue = await executor.ExecuteAsync((a, t) => Task.FromResult(expectedValue), null);

            // Assert
            actualValue.ShouldBe(expectedValue);
            _activityPersistence.ActivitySummary.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityPersistence.ActivitySummary.Instance.Id));
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
            var executor = new ActivityExecutor(_workflowVersionMock.Object, _asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityPersistence, executor, null);
            _activityPersistence.ActivitySummary.Version.FailUrgency = ActivityFailUrgencyEnum.Stopping;

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
            _activityPersistence.ActivitySummary.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityPersistence.ActivitySummary.Instance.Id));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsAndStopping_Gives_AlertHandlerCalled()
        {
            // Arrange
            var alertHandler = new WorkflowVersionWithAlertHandler((a, ct) => Task.FromResult(true));
            var executor = new ActivityExecutor(alertHandler, _asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityPersistence, executor, null);
            _activityPersistence.ActivitySummary.Version.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            await Assert.ThrowsAnyAsync<RequestPostponedException>( () => executor.ExecuteAsync<int>(
                    (a, t) => throw new Exception("Fail"), null));
            alertHandler.AlertResult.ShouldBe(true);
            _activityPersistence.ActivitySummary.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityPersistence.ActivitySummary.Instance.Id));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
            instance.ExceptionAlertHandled.ShouldBe(true);
        }

        [Theory]
        [InlineData(ActivityFailUrgencyEnum.HandleLater)]
        [InlineData(ActivityFailUrgencyEnum.Ignore)]
        public async Task Execute_Given_MethodThrowsAndNotStopping_Gives_Default(ActivityFailUrgencyEnum failUrgency)
        {
            // Arrange
            var executor = new ActivityExecutor(_workflowVersionMock.Object, _asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityPersistence, executor, null);
            _activityPersistence.ActivitySummary.Version.FailUrgency = failUrgency;
            const int expectedValue = 10;

            // Act
            var actualValue = await executor.ExecuteAsync(
                (a, t) => throw new Exception("Fail"), ct => Task.FromResult(expectedValue));
            _activityPersistence.ActivitySummary.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityPersistence.ActivitySummary.Instance.Id));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
            actualValue.ShouldBe(expectedValue);
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsRequestPostponed_Gives_RequestIdSet()
        {
            // Arrange
            var executor = new ActivityExecutor(_workflowVersionMock.Object, _asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityPersistence, executor, null);
            var expectedRequestId = Guid.NewGuid().ToString();

            // Act & Assert
            await Assert.ThrowsAnyAsync<RequestPostponedException>(
                () => executor.ExecuteAsync<int>(
                    (a, t) => throw new RequestPostponedException(expectedRequestId), null));
            _activityPersistence.ActivitySummary.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityPersistence.ActivitySummary.Instance.Id));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Waiting.ToString());
            instance.AsyncRequestId.ShouldBe(expectedRequestId);
        }

        [Fact]
        public async Task Execute_Given_HasRequestIdButNoResponse_Gives_Postpone()
        {
            // Arrange
            var expectedRequestId = Guid.NewGuid().ToString();
            _asyncRequestClientMock.Setup(c =>
                    c.SendRequestAsync(It.IsAny<AsyncHttpRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRequestId);
            _asyncRequestClientMock.Setup(c =>
                    c.GetFinalResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AsyncHttpResponse) null);
            _asyncRequestClientMock.Setup(c =>
                    c.CreateRequest(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<double>()))
                .Returns(new AsyncHttpRequest_ForTest(_asyncRequestClientMock.Object, HttpMethod.Post, "http://example.com",
                    1.0));
            var executor = new ActivityExecutor(_workflowVersionMock.Object, _asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityPersistence, executor, null);
            await Assert.ThrowsAnyAsync<RequestPostponedException>(
                () => executor.ExecuteAsync<int>(
                    (a, t) => throw new RequestPostponedException(expectedRequestId), null));
            executor = new ActivityExecutor(_workflowVersionMock.Object, _asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityPersistence, executor, null);

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
        }

        [Fact]
        public async Task Execute_Given_FuclrumTryAgainException_Gives_PostponeTryAgain()
        {
            // Arrange
            var executor = new ActivityExecutor(_workflowVersionMock.Object, _asyncRequestClientMock.Object);
            executor.Activity = new ActivityAction<int>(_activityPersistence, executor, null);
            _activityPersistence.ActivitySummary.Version.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            RequestPostponedException postponed = null;
            try
            {
                await executor.ExecuteAsync<int>(
                    (a, t) => throw new FulcrumTryAgainException("Fail"), null);
            }
            catch (Exception e)
            {
                e.ShouldBeAssignableTo<RequestPostponedException>();
                postponed = e as RequestPostponedException;
            }
            postponed.ShouldNotBeNull();
            postponed.TryAgain.ShouldBe(true);
            _activityPersistence.ActivitySummary.Instance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(_activityPersistence.ActivitySummary.Instance.Id));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Waiting.ToString());
        }
    }
    /// <summary>
    /// Class to access protected parts of the <see cref="AsyncHttpRequest"/> class.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class AsyncHttpRequest_ForTest : AsyncHttpRequest
    {
        /// <inheritdoc />
        public AsyncHttpRequest_ForTest(IAsyncRequestClient asyncRequestClient, HttpMethod method, string url, double priority) : base(asyncRequestClient, method, url, priority)
        {
        }

        /// <inheritdoc />
        public AsyncHttpRequest_ForTest(HttpMethod method, string url, double priority) : base(new Mock<IAsyncRequestClient>().Object, method, url, priority)
        {
        }
    }

    internal class WorkflowVersionWithAlertHandler : IWorkflowVersionBase, IActivityExceptionAlertHandler
    {
        private readonly Func<ActivityExceptionAlert, CancellationToken, Task<bool>> _alertHandlerMethod;

        public WorkflowVersionWithAlertHandler(Func<ActivityExceptionAlert, CancellationToken, Task<bool>> alertHandlerMethod)
        {
            _alertHandlerMethod = alertHandlerMethod;
            MajorVersion = 1;
            MinorVersion = 0;
        }

        /// <inheritdoc />
        public int MajorVersion { get; }

        /// <inheritdoc />
        public int MinorVersion { get; }

        /// <inheritdoc />
        public async Task<bool> HandleActivityExceptionAlertAsync(ActivityExceptionAlert alert, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _alertHandlerMethod(alert, cancellationToken);
                AlertResult = result;
                return result;
            }
            catch (Exception e)
            {
                AlertException = e;
                throw;
            }
        }

        public bool? AlertResult { get; private set; }

        public Exception AlertException { get; private set; }
    }
}

