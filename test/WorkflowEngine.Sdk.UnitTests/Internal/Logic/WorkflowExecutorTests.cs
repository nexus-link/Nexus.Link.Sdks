using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Messages;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Queue.Logic;
using Nexus.Link.Libraries.Core.Queue.Model;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic
{
    public class WorkflowExecutorTests
    {
        private readonly IConfigurationTables _configurationTables;
        private readonly IRuntimeTables _runtimeTables;
        private readonly WorkflowCapabilities _workflowCapabilities;
        private readonly WorkflowCapabilities _workflowCapabilitiesWithEvents;
        private readonly AsyncRequestMgmtMock _armMock;
        private readonly Mock<IWritableQueue<WorkflowInstanceChangedV1>> _messageQueueMock;

        private const string SourceClientId = "mock-source";

        public WorkflowExecutorTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityExecutorTests));
            FulcrumApplication.Setup.ClientName = SourceClientId;

            _armMock = new AsyncRequestMgmtMock();
            _configurationTables = new ConfigurationTablesMemory();
            _runtimeTables = new RuntimeTablesMemory();
            _workflowCapabilities = new WorkflowCapabilities(_configurationTables, _runtimeTables, _armMock);
            _messageQueueMock = new Mock<IWritableQueue<WorkflowInstanceChangedV1>>();
            _workflowCapabilitiesWithEvents = new WorkflowCapabilities(_configurationTables, _runtimeTables, _armMock, _messageQueueMock.Object);
        }

        [Fact]
        public async Task Execute_Given_FirstTime_Gives_CreateInstance()
        {
            // Arrange
            var expectedRequestId = Guid.NewGuid();
            FulcrumApplication.Context.ExecutionId = expectedRequestId.ToGuidString();
            var implementation = new TestWorkflowImplementation(_workflowCapabilities,
                _ => throw new RequestPostponedException());
            var information = new WorkflowInformation(implementation);
            var executor = new WorkflowExecutor(information, _workflowCapabilities);

            // Act
            var exception = await executor.ExecuteAsync(implementation, new CancellationToken())
                .ShouldThrowAsync<RequestPostponedException>();

            // Assert
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(expectedRequestId);
            instance.ShouldNotBeNull();
        }

        [Fact]
        public async Task Execute_Given_WorkflowThrowsWorkflowFastForwardBreak_Gives_ThrowsSameException()
        {
            // Arrange
            FulcrumApplication.Context.ExecutionId = Guid.NewGuid().ToString();
            var expectedTechnicalMessage = Guid.NewGuid().ToGuidString();
            var expectedFriendlyMessage = Guid.NewGuid().ToGuidString();
            var implementation = new TestWorkflowImplementation(_workflowCapabilities,
                _ => throw new WorkflowImplementationShouldNotCatchThisException(new WorkflowFastForwardBreakException()));
            var information = new WorkflowInformation(implementation);
            var executor = new WorkflowExecutor(information, _workflowCapabilities);

            // Act & Assert
            var exception = await executor.ExecuteAsync(implementation, new CancellationToken())
                .ShouldThrowAsync<WorkflowFastForwardBreakException>();
        }

        [Fact]
        public async Task Execute_Given_WorkflowThrowsWorkflowFailed_Gives_ThrowsWorkflowCancelled()
        {
            // Arrange
            FulcrumApplication.Context.ExecutionId = Guid.NewGuid().ToString();
            var expectedTechnicalMessage = Guid.NewGuid().ToGuidString();
            var expectedFriendlyMessage = Guid.NewGuid().ToGuidString();
            var implementation = new TestWorkflowImplementation(_workflowCapabilities,
                _ => throw new WorkflowImplementationShouldNotCatchThisException(new WorkflowFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError, expectedTechnicalMessage, expectedFriendlyMessage)));
            var information = new WorkflowInformation(implementation);
            var executor = new WorkflowExecutor(information, _workflowCapabilities);

            // Act
            var exception = await executor.ExecuteAsync(implementation, new CancellationToken())
                .ShouldThrowAsync<FulcrumCancelledException>();

            // Assert
            exception.TechnicalMessage.ShouldBe(expectedTechnicalMessage);
            exception.FriendlyMessage.ShouldBe(expectedFriendlyMessage);
        }

        [Fact]
        public async Task Execute_Given_WorkflowThrowsOtherException_Gives_ThrowsPostponed()
        {
            // Arrange
            FulcrumApplication.Context.ExecutionId = Guid.NewGuid().ToGuidString();
            FulcrumApplication.Context.ReentryAuthentication = Guid.NewGuid().ToGuidString();
            var implementation = new TestWorkflowImplementation(_workflowCapabilities,
                _ => Task.CompletedTask);
            var information = new WorkflowInformation(implementation);
            var executor = new WorkflowExecutor(information, _workflowCapabilities);
            await executor.ExecuteAsync(implementation, new CancellationToken());
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(FulcrumApplication.Context.ExecutionId.ToGuid());
            implementation = new TestWorkflowImplementation(_workflowCapabilities,
                _ => throw new Exception());
            information = new WorkflowInformation(implementation);
            executor = new WorkflowExecutor(information, _workflowCapabilities);

            // Act
            var exception = await executor.ExecuteAsync(implementation, new CancellationToken())
                .ShouldThrowAsync<RequestPostponedException>();
        }

        [Fact]
        public async Task Execute_Given_WorkflowThrowsTransportWithOtherException_Gives_ThrowsPostponed()
        {
            // Arrange
            FulcrumApplication.Context.ExecutionId = Guid.NewGuid().ToString();
            FulcrumApplication.Context.ReentryAuthentication = Guid.NewGuid().ToGuidString();
            var implementation = new TestWorkflowImplementation(_workflowCapabilities,
                _ => Task.CompletedTask);
            var information = new WorkflowInformation(implementation);
            var executor = new WorkflowExecutor(information, _workflowCapabilities);
            await executor.ExecuteAsync(implementation, new CancellationToken());
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(FulcrumApplication.Context.ExecutionId.ToGuid());
            implementation = new TestWorkflowImplementation(_workflowCapabilities,
                _ => throw new WorkflowImplementationShouldNotCatchThisException(new Exception()));
            information = new WorkflowInformation(implementation);
            executor = new WorkflowExecutor(information, _workflowCapabilities);

            // Act
            var exception = await executor.ExecuteAsync(implementation, new CancellationToken())
                .ShouldThrowAsync<RequestPostponedException>();
        }

        [Fact]
        public async Task Execute_Given_FirstTime_Fires_Event()
        {
            // Arrange
            var expectedRequestId = Guid.NewGuid();
            FulcrumApplication.Context.ExecutionId = expectedRequestId.ToGuidString();
            var implementation = new TestWorkflowImplementation(_workflowCapabilitiesWithEvents, ct => throw new RequestPostponedException());
            var information = new WorkflowInformation(implementation);
            var executor = new WorkflowExecutor(information, _workflowCapabilitiesWithEvents);

            var resetEvent = new ManualResetEvent(false);
            _messageQueueMock
                .Setup(x => x.AddMessageAsync(
                    It.Is<WorkflowInstanceChangedV1>(message =>
                        string.Equals(message.Instance.Id, expectedRequestId.ToString(),
                            StringComparison.InvariantCultureIgnoreCase)
                        && message.SourceClientId == SourceClientId),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken>()))
                .Callback((WorkflowInstanceChangedV1 x, TimeSpan? y, CancellationToken z) =>
                {
                    resetEvent.Set();
                });

            // Act
            await executor
                .ExecuteAsync(implementation, new CancellationToken())
                .ShouldThrowAsync<RequestPostponedException>();

            // Assert
            resetEvent.WaitOne(300).ShouldBeTrue();
            _messageQueueMock.VerifyAll();
        }

        [Fact]
        public async Task Execute_Given_Instance_Changes_State_Fires_Event()
        {
            // Arrange
            var expectedRequestId = Guid.NewGuid();
            FulcrumApplication.Context.ExecutionId = expectedRequestId.ToGuidString();

            var implementation = new TestWorkflowImplementation(_workflowCapabilitiesWithEvents, ct => throw new RequestPostponedException());
            var information = new WorkflowInformation(implementation);
            var executor = new WorkflowExecutor(information, _workflowCapabilitiesWithEvents);

            var cancellationToken1 = new CancellationTokenSource().Token;
            var cancellationToken2 = new CancellationTokenSource().Token;
            var resetEvent1 = new ManualResetEvent(false);
            var resetEvent2 = new ManualResetEvent(false);

            var newFormTitle = Guid.NewGuid().ToString();

            _messageQueueMock
                .Setup(x => x.AddMessageAsync(
                    It.Is<WorkflowInstanceChangedV1>(message => 
                        string.Equals(message.Instance.Id, expectedRequestId.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && message.SourceClientId == SourceClientId
                        && message.Form.Title == information.FormTitle),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback((WorkflowInstanceChangedV1 x, TimeSpan? y, CancellationToken z) =>
                {
                    resetEvent1.Set();
                });
            _messageQueueMock
                .Setup(x => x.AddMessageAsync(
                    It.Is<WorkflowInstanceChangedV1>(message => 
                        string.Equals(message.Instance.Id, expectedRequestId.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && message.SourceClientId == SourceClientId
                        && message.Form.Title == newFormTitle),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback((WorkflowInstanceChangedV1 x, TimeSpan? y, CancellationToken z) =>
                {
                    resetEvent2.Set();
                });

            // Act
            await executor
                .ExecuteAsync(implementation, cancellationToken1)
                .ShouldThrowAsync<RequestPostponedException>();

            // TODO: How to change state
            //((TestWorkflowContainer) implementation.WorkflowContainer).SetWorkflowFormTitle(newFormTitle);
            information.Form.Title = newFormTitle;
            
            await executor
                .ExecuteAsync(implementation, cancellationToken2)
                .ShouldThrowAsync<RequestPostponedException>();

            // Assert
            resetEvent1.WaitOne(300).ShouldBeTrue();
            resetEvent2.WaitOne(300).ShouldBeTrue();

            //_messageQueueMock.Verify(x => x.AddMessageAsync(
            //    It.Is<WorkflowInstanceChangedV1>(message =>
            //        string.Equals(message.Payload.Instance.Id, expectedRequestId.ToString(), StringComparison.InvariantCultureIgnoreCase)
            //        && message.Payload.SourceClientId == SourceClientId),
            //    It.IsAny<TimeSpan?>(),
            //    cancelationToken2));
            _messageQueueMock.VerifyAll();
        }
    }
}

