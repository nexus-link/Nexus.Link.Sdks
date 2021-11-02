using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.ActivityLogic;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;
using Nexus.Link.WorkflowEngine.Sdk.Persistence;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Support;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
{
    public class ActivityExecutorTests
    {
        private readonly Mock<IAsyncRequestClient> _asyncRequestClientMock;
        private readonly IRuntimeTables _runtimeTables;
        private readonly Mock<IWorkflowVersion> _workflowVersionMock;
        private readonly IInternalActivityFlow _activityFlowMock;
        private readonly WorkflowCache _workflowCache;
        private readonly WorkflowCapability _workflowCapability;

        public ActivityExecutorTests()
        {
            var configurationTables = new ConfigurationTablesMemory();
            _runtimeTables = new RuntimeTablesMemory();
            
            var asyncRequestMgmtCapabilityMock = new Mock<IAsyncRequestMgmtCapability>();
            _workflowCapability = new WorkflowCapability(configurationTables, _runtimeTables, asyncRequestMgmtCapabilityMock.Object);
            var workflowInfo = new WorkflowInformation
            {
                WorkflowCapability = _workflowCapability,
                CapabilityName = "Capability name",
                FormId = Guid.NewGuid().ToLowerCaseString(),
                FormTitle = "Form title",
                MajorVersion = 1,
                MinorVersion = 2,
                InstanceId = Guid.NewGuid().ToLowerCaseString(),
                InstanceTitle = "Instance title"
            };
            _workflowCache = new WorkflowCache(workflowInfo);
            _workflowCache.LoadAsync(default).Wait();
            _workflowVersionMock = new Mock<IWorkflowVersion>();
            _asyncRequestClientMock = new Mock<IAsyncRequestClient>();
            _activityFlowMock = new ActivityFlowMock(_workflowVersionMock.Object,
                _workflowCache, "Form title", "0D759290-9F93-4B3A-8333-76019DE227CF", 1);
        }

        [Fact]
        public async Task Execute_Given_MethodReturns_Gives_Success()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityFlowMock, null);
            var executor = new ActivityExecutor(_workflowVersionMock.Object, activity);
            const int expectedValue = 10;
            var minTime = DateTimeOffset.UtcNow;

            // Act
            var actualValue = await executor.ExecuteAsync((a, t) => Task.FromResult(expectedValue), null);
            await _workflowCache.SaveAsync();

            // Assert
            actualValue.ShouldBe(expectedValue);
            executor.ActivityInstance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(executor.ActivityInstance.Id));
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
            var executor = new ActivityExecutor(_workflowVersionMock.Object, activity);
            executor.ActivityVersion.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            RequestPostponedException postponed = null;
            try
            {
                await executor.ExecuteAsync(
                    (a, t) => throw new Exception("Fail"));
            }
            catch (Exception e)
            {
                e.ShouldBeAssignableTo<RequestPostponedException>();
                postponed = e as RequestPostponedException;
            }
            await _workflowCache.SaveAsync();
            postponed.ShouldNotBeNull();
            executor.ActivityInstance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(executor.ActivityInstance.Id));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Failed.ToString());
        }

        [Fact]
        public async Task Execute_Given_MethodThrowsAndStopping_Gives_AlertHandlerCalled()
        {
            // Arrange
            var alertHandler = new WorkflowVersionWithAlertHandler((a, ct) => Task.FromResult(true));
            var activityFlowMock = new ActivityFlowMock(alertHandler,
                _workflowCache, "Form title", "0D759290-9F93-4B3A-8333-76019DE227CF", 1);
            var activity = new ActivityAction<int>(activityFlowMock, null);
            var executor = new ActivityExecutor(alertHandler, activity);
            executor.ActivityVersion.FailUrgency = ActivityFailUrgencyEnum.Stopping;

            // Act & Assert
            await Assert.ThrowsAnyAsync<RequestPostponedException>( () => executor.ExecuteAsync(
                    (a, t) => throw new Exception("Fail")));
            await _workflowCache.SaveAsync();

            alertHandler.AlertResult.ShouldBe(true);
            executor.ActivityInstance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(executor.ActivityInstance.Id));
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
            var activity = new ActivityAction<int>(_activityFlowMock, null);
            var executor = new ActivityExecutor(_workflowVersionMock.Object, activity);
            executor.ActivityVersion.FailUrgency = failUrgency;
            const int expectedValue = 10;

            // Act
            var actualValue = await executor.ExecuteAsync(
                (a, t) => throw new Exception("Fail"), ct => Task.FromResult(expectedValue));
            await _workflowCache.SaveAsync();
            executor.ActivityInstance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(executor.ActivityInstance.Id));
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
            var executor = new ActivityExecutor(_workflowVersionMock.Object, activity);
            var expectedRequestId = Guid.NewGuid().ToLowerCaseString();

            // Act & Assert
            await Assert.ThrowsAnyAsync<RequestPostponedException>(
                () => executor.ExecuteAsync<int>(
                    (a, t) => throw new RequestPostponedException(expectedRequestId), null));
            await _workflowCache.SaveAsync();
            executor.ActivityInstance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(executor.ActivityInstance.Id));
            instance.ShouldNotBeNull();
            instance.State.ShouldBe(ActivityStateEnum.Waiting.ToString());
            instance.AsyncRequestId.ShouldBe(expectedRequestId);
        }

        [Fact]
        public async Task Execute_Given_HasRequestIdButNoResponse_Gives_Postpone()
        {
            // Arrange
            var expectedRequestId = Guid.NewGuid().ToLowerCaseString();
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
            var activity = new ActivityAction<int>(_activityFlowMock, null);
            var executor = new ActivityExecutor(_workflowVersionMock.Object, activity);
            await Assert.ThrowsAnyAsync<RequestPostponedException>(
                () => executor.ExecuteAsync<int>(
                    (a, t) => throw new RequestPostponedException(expectedRequestId), null));
            activity = new ActivityAction<int>(_activityFlowMock, null);
            executor = new ActivityExecutor(_workflowVersionMock.Object, activity);

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
            await _workflowCache.SaveAsync();
            postponed.ShouldNotBeNull();
            postponed.WaitingForRequestIds.ShouldContain(expectedRequestId);
        }

        [Fact]
        public async Task Execute_Given_FuclrumTryAgainException_Gives_PostponeTryAgain()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityFlowMock, null);
            var executor = new ActivityExecutor(_workflowVersionMock.Object, activity);
            executor.ActivityVersion.FailUrgency = ActivityFailUrgencyEnum.Stopping;

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
            await _workflowCache.SaveAsync();
            postponed.ShouldNotBeNull();
            postponed.TryAgain.ShouldBe(true);
            executor.ActivityInstance.Id.ShouldNotBeNull();
            var instance = await _runtimeTables.ActivityInstance.ReadAsync(MapperHelper.MapToType<Guid, string>(executor.ActivityInstance.Id));
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

    internal class WorkflowVersionWithAlertHandler : IWorkflowVersion, IActivityExceptionAlertHandler
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

