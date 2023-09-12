using System;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic;

public class WorkflowBeforeAndAfterTests
{
    private readonly IConfigurationTables _configurationTables;
    private readonly IRuntimeTables _runtimeTables;
    private readonly WorkflowCapabilities _workflowCapabilities;
    private readonly AsyncRequestMgmtMock _armMock;
    private readonly WorkflowEngineStorageMemory _storage;

    private const string SourceClientId = "mock-source";

    public WorkflowBeforeAndAfterTests()
    {
        FulcrumApplicationHelper.UnitTestSetup(nameof(WorkflowBeforeAndAfterTests));
        FulcrumApplication.Setup.ClientName = SourceClientId;

        _armMock = new AsyncRequestMgmtMock();
        _configurationTables = new ConfigurationTablesMemory();
        _runtimeTables = new RuntimeTablesMemory();
        _storage = new WorkflowEngineStorageMemory();
        _workflowCapabilities = new WorkflowCapabilities(_configurationTables, _runtimeTables, _armMock, _storage);
    }

    [Fact]
    public async Task BeforeExecution_Given_FirstTime_Gives_CreateInstance()
    {
        // Arrange
        var expectedRequestId = Guid.NewGuid();
        FulcrumApplication.Context.ExecutionId = expectedRequestId.ToGuidString();
        var implementation = new TestWorkflowImplementation(_workflowCapabilities,
            _ => throw new WorkflowImplementationShouldNotCatchThisException(new ActivityPostponedException(null)));
        var information = new WorkflowInformation(implementation);
        var beforeAndAfter = new WorkflowBeforeAndAfterExecution(information);

        // Act
        await beforeAndAfter.BeforeExecutionAsync(default);

        // Assert
        var instance = await _runtimeTables.WorkflowInstance.ReadAsync(expectedRequestId);
        instance.ShouldNotBeNull();
    }

    [Fact]
    public async Task BeforeExecution_Given_DatabaseFails_Gives_ThrowsPostponeWithRetry()
    {
        // Arrange
        var expectedRequestId = Guid.NewGuid();
        FulcrumApplication.Context.ExecutionId = expectedRequestId.ToGuidString();
        var configurationTablesMock = new Mock<IConfigurationTables>();
        configurationTablesMock
            .SetupGet(t => t.WorkflowForm)
            .Throws(new FulcrumResourceException());
        var runtimeTablesMock = new Mock<IRuntimeTables>();
        var storageMock = new Mock<IWorkflowEngineStorage>();
        var workflowCapabilities = new WorkflowCapabilities(configurationTablesMock.Object, runtimeTablesMock.Object, _armMock, storageMock.Object);
        var implementation = new TestWorkflowImplementation(workflowCapabilities,
            _ => throw new WorkflowImplementationShouldNotCatchThisException(new ActivityPostponedException(null)));
        var information = new WorkflowInformation(implementation);
        var beforeAndAfter = new WorkflowBeforeAndAfterExecution(information);

        // Act
        var exception = await beforeAndAfter.BeforeExecutionAsync(default)
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert

    }
}