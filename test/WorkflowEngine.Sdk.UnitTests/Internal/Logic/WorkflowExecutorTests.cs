using System;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic;

public class WorkflowExecutorTests
{
    private readonly IConfigurationTables _configurationTables;
    private readonly IRuntimeTables _runtimeTables;
    private readonly WorkflowCapabilities _workflowCapabilities;
    private readonly AsyncRequestMgmtMock _armMock;
    private readonly WorkflowEngineStorageMemory _storage;
    private readonly Mock<IWorkflowBeforeAndAfterExecution> _beforeAndAfterMock;

    private const string SourceClientId = "mock-source";

    public WorkflowExecutorTests()
    {
        FulcrumApplicationHelper.UnitTestSetup(nameof(WorkflowExecutorTests));
        FulcrumApplication.Setup.ClientName = SourceClientId;

        _armMock = new AsyncRequestMgmtMock();
        _configurationTables = new ConfigurationTablesMemory();
        _runtimeTables = new RuntimeTablesMemory();
        _storage = new WorkflowEngineStorageMemory();
        _workflowCapabilities = new WorkflowCapabilities(_configurationTables, _runtimeTables, _armMock, _storage);
        _beforeAndAfterMock = new Mock<IWorkflowBeforeAndAfterExecution>();
    }

    [Fact]
    public async Task Execute_Given_WorkflowThrowsWorkflowFastForwardBreak_Gives_ThrowsSameException()
    {
        // Arrange
        FulcrumApplication.Context.ExecutionId = Guid.NewGuid().ToString();
        var implementation = new TestWorkflowImplementation(_workflowCapabilities,
            _ => throw new WorkflowImplementationShouldNotCatchThisException(new WorkflowFastForwardBreakException()));
        var information = new WorkflowInformation(implementation);
        var executor = new WorkflowExecutor(information, _beforeAndAfterMock.Object);

        // Act & Assert
        var exception = await executor.ExecuteAsync(implementation, default)
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
        var information = new WorkflowInformationMock(new WorkflowForm(), new WorkflowVersion(), new WorkflowInstance());
        var executor = new WorkflowExecutor(information, _beforeAndAfterMock.Object);

        // Act
        var exception = await executor.ExecuteAsync(implementation, default)
            .ShouldThrowAsync<FulcrumCancelledException>();

        // Assert
        exception.TechnicalMessage.ShouldBe(expectedTechnicalMessage);
        exception.FriendlyMessage.ShouldBe(expectedFriendlyMessage);
    }

    [Fact]
    public async Task ExecuteR_Given_WorkflowThrowsWorkflowFailed_Gives_ThrowsCancelledException()
    {
        // Arrange
        FulcrumApplication.Context.ExecutionId = Guid.NewGuid().ToString();
        var expectedTechnicalMessage = Guid.NewGuid().ToGuidString();
        var expectedFriendlyMessage = Guid.NewGuid().ToGuidString();
        var implementation = new TestWorkflowImplementationR(_workflowCapabilities,
            _ => throw new WorkflowImplementationShouldNotCatchThisException(new WorkflowFailedException(ActivityExceptionCategoryEnum.WorkflowCapabilityError, expectedTechnicalMessage, expectedFriendlyMessage)));
        var information = new WorkflowInformationMock(new WorkflowForm(), new WorkflowVersion(), new WorkflowInstance());
        var executor = new WorkflowExecutor(information, _beforeAndAfterMock.Object);

        // Act
        var exception = await executor.ExecuteAsync(implementation, default)
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
        var information = new WorkflowInformationMock(new WorkflowForm(), new WorkflowVersion(), new WorkflowInstance());
        var executor = new WorkflowExecutor(information, _beforeAndAfterMock.Object);
        await executor.ExecuteAsync(implementation, default);
        implementation = new TestWorkflowImplementation(_workflowCapabilities, _ => throw new FulcrumResourceException());
        executor = new WorkflowExecutor(information, _beforeAndAfterMock.Object);

        // Act
        var exception = await executor.ExecuteAsync(implementation, default)
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        exception.TryAgain.ShouldBe(false);
    }

    [Fact]
    public async Task Execute_Given_WorkflowThrowsTransportWithOtherException_Gives_ThrowsPostponed()
    {
        // Arrange
        FulcrumApplication.Context.ExecutionId = Guid.NewGuid().ToString();
        FulcrumApplication.Context.ReentryAuthentication = Guid.NewGuid().ToGuidString();
        var implementation = new TestWorkflowImplementation(_workflowCapabilities,
            _ => Task.CompletedTask);
        var information = new WorkflowInformationMock(new WorkflowForm(), new WorkflowVersion(), new WorkflowInstance());
        var executor = new WorkflowExecutor(information, _beforeAndAfterMock.Object);
        await executor.ExecuteAsync(implementation, default);
        implementation = new TestWorkflowImplementation(_workflowCapabilities,
            _ => throw new WorkflowImplementationShouldNotCatchThisException(new Exception()));
        executor = new WorkflowExecutor(information, _beforeAndAfterMock.Object);

        // Act
        var exception = await executor.ExecuteAsync(implementation, default)
            .ShouldThrowAsync<RequestPostponedException>();
    }
}