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

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.OneAction.Support;

public abstract class Base
{
    private readonly WorkflowEngineStorageMemory _storage;
    protected Mock<IMyLogic> LogicMoq { get; }
    protected ConfigurationTablesMemory ConfigurationTables { get; }
    protected RuntimeTablesMemory RuntimeTables { get; }
    protected AsyncRequestMgmtMock AsyncRequestMgmtMock { get; }
    protected WorkflowContainer WorkflowContainer{ get; }
    protected Fixture DataFixture { get; }
    protected Guid WorkflowInstanceId { get; }
    protected IWorkflowMgmtCapability WorkflowMgmtCapability { get; }

    protected Base(string name)
    {
        FulcrumApplicationHelper.UnitTestSetup(name);
        DataFixture = new Fixture();
        ConfigurationTables = new ConfigurationTablesMemory();
        RuntimeTables = new RuntimeTablesMemory();
        _storage = new WorkflowEngineStorageMemory();
        AsyncRequestMgmtMock = new AsyncRequestMgmtMock();
        var workflowCapabilities = new WorkflowCapabilities(ConfigurationTables, RuntimeTables, AsyncRequestMgmtMock, _storage);
        WorkflowContainer = new MyWorkflowContainer(workflowCapabilities);
        LogicMoq = new Mock<IMyLogic>();
        WorkflowContainer.AddImplementation(new OneActionImplementation(WorkflowContainer, LogicMoq.Object));
        WorkflowInstanceId = DataFixture.Create<Guid>();
        FulcrumApplication.Context.ExecutionId = WorkflowInstanceId.ToGuidString();
        FulcrumApplication.Context.ManagedAsynchronousRequestId = DataFixture.Create<Guid>().ToGuidString();
        WorkflowMgmtCapability = new WorkflowMgmtCapability(workflowCapabilities, RuntimeTables, ConfigurationTables);
    }

}