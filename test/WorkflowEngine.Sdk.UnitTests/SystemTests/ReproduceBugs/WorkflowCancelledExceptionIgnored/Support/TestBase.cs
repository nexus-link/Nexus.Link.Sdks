using System;
using AutoFixture;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.Sdk.UnitTests.TestSupport;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.ReproduceBugs.WorkflowCancelledExceptionIgnored.Support;

public abstract class TestBase
{
    protected WorkflowEngineStorageMemory WorkflowEngineStorage { get; }
    protected Mock<IIutWorkflowLogic> LogicMoq { get; }
    protected ConfigurationTablesMemory ConfigurationTables { get; }
    protected RuntimeTablesMemory RuntimeTables { get; }
    protected AsyncRequestMgmtMock AsyncRequestMgmtMock { get; }
    protected WorkflowContainer WorkflowContainer{ get; }
    protected Fixture DataFixture { get; }
    protected Guid WorkflowInstanceId { get; }
    protected IWorkflowMgmtCapability WorkflowMgmtCapability { get; }

    protected TestBase(string name)
    {
        FulcrumApplicationHelper.UnitTestSetup(name);
        DataFixture = new Fixture();
        ConfigurationTables = new ConfigurationTablesMemory();
        RuntimeTables = new RuntimeTablesMemory();
        WorkflowEngineStorage = new WorkflowEngineStorageMemory();
        AsyncRequestMgmtMock = new AsyncRequestMgmtMock();
        var workflowCapabilities = new WorkflowCapabilities(ConfigurationTables, RuntimeTables, AsyncRequestMgmtMock, WorkflowEngineStorage);
        WorkflowContainer = new IutWorkflowContainer(workflowCapabilities);
        LogicMoq = new Mock<IIutWorkflowLogic>();
        WorkflowContainer.AddImplementation(new IutWorkflowImplementation(WorkflowContainer, LogicMoq.Object));
        WorkflowInstanceId = DataFixture.Create<Guid>();
        FulcrumApplication.Context.ExecutionId = WorkflowInstanceId.ToGuidString();
        FulcrumApplication.Context.ManagedAsynchronousRequestId = DataFixture.Create<Guid>().ToGuidString();
        WorkflowMgmtCapability = new WorkflowMgmtCapability(workflowCapabilities, RuntimeTables, ConfigurationTables);
    }

}