using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic
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
        }

        [Fact]
        public async Task Execute_Given_FirstTime_Gives_CreateInstance()
        {
            // Arrange
            var expectedRequestId = Guid.NewGuid();
            FulcrumApplication.Context.ExecutionId = expectedRequestId.ToGuidString();
            var implementation = new TestWorkflowImplementation(_workflowCapabilities,
                ct => throw new RequestPostponedException());
            var executor = new WorkflowExecutor(implementation);

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
                ct => throw new ExceptionTransporter(new WorkflowFastForwardBreakException()));
            var executor = new WorkflowExecutor(implementation);

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
                ct => throw new ExceptionTransporter(new WorkflowFailedException(ActivityExceptionCategoryEnum.TechnicalError, expectedTechnicalMessage, expectedFriendlyMessage)));
            var executor = new WorkflowExecutor(implementation);

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
                ct => Task.CompletedTask);
            var executor = new WorkflowExecutor(implementation);
            await executor.ExecuteAsync(implementation, new CancellationToken());
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(FulcrumApplication.Context.ExecutionId.ToGuid());
            implementation = new TestWorkflowImplementation(_workflowCapabilities,
                ct => throw new Exception());
            executor = new WorkflowExecutor(implementation);

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
                ct => Task.CompletedTask);
            var executor = new WorkflowExecutor(implementation);
            await executor.ExecuteAsync(implementation, new CancellationToken());
            var instance = await _runtimeTables.WorkflowInstance.ReadAsync(FulcrumApplication.Context.ExecutionId.ToGuid());
            implementation = new TestWorkflowImplementation(_workflowCapabilities,
                ct => throw new ExceptionTransporter(new Exception()));
            executor = new WorkflowExecutor(implementation);

            // Act
            var exception = await executor.ExecuteAsync(implementation, new CancellationToken())
                .ShouldThrowAsync<RequestPostponedException>();
        }
    }
}
