using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
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
        private readonly AsyncRequestMgmtMock _armMock;

        public WorkflowExecutorTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityExecutorTests));
            
            _armMock = new AsyncRequestMgmtMock();
            _configurationTables = new ConfigurationTablesMemory();
            _runtimeTables = new RuntimeTablesMemory();
            _workflowCapabilities = new WorkflowCapabilities(_configurationTables, _runtimeTables, _armMock);
            // TODO: queue
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
            var executor = new WorkflowExecutor(information);

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
            var executor = new WorkflowExecutor(information);

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
            var executor = new WorkflowExecutor(information);

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
            var executor = new WorkflowExecutor(information);
            await executor.ExecuteAsync(implementation, new CancellationToken());
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(FulcrumApplication.Context.ExecutionId.ToGuid());
            implementation = new TestWorkflowImplementation(_workflowCapabilities,
                _ => throw new Exception());
            information = new WorkflowInformation(implementation);
            executor = new WorkflowExecutor(information);

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
            var executor = new WorkflowExecutor(information);
            await executor.ExecuteAsync(implementation, new CancellationToken());
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(FulcrumApplication.Context.ExecutionId.ToGuid());
            implementation = new TestWorkflowImplementation(_workflowCapabilities,
                _ => throw new WorkflowImplementationShouldNotCatchThisException(new Exception()));
            information = new WorkflowInformation(implementation);
            executor = new WorkflowExecutor(information);

            // Act
            var exception = await executor.ExecuteAsync(implementation, new CancellationToken())
                .ShouldThrowAsync<RequestPostponedException>();
        }
    }
}

