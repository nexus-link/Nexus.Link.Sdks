using System;
using AutoFixture;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Containers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory.Tables;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.Sdk.UnitTests.TestSupport;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.DBFallback.Support;

public abstract class Base
{
    public IWorkflowEngineStorage WorkflowEngineStorage { get; }
    protected Mock<IProvokePersistenceFailures> LogicMoq { get; }
    protected ConfigurationTablesMemory ConfigurationTables { get; }
    protected RuntimeTablesMemory RuntimeTables { get; }
    protected WorkflowInstanceTableMemory WorkflowInstanceTable { get; }
    protected WorkflowSummaryStoreMemory WorkflowSummaryStore { get; }
    protected AsyncRequestMgmtMock AsyncRequestMgmtMock { get; }
    protected WorkflowContainer WorkflowContainer{ get; }
    protected Fixture DataFixture { get; }
    protected Guid WorkflowInstanceId { get; }
    protected IWorkflowMgmtCapability WorkflowMgmtCapability { get; }

    protected Base(string name, bool useStorage)
    {
        FulcrumApplicationHelper.UnitTestSetup(name);
        DataFixture = new Fixture();
        ConfigurationTables = new ConfigurationTablesMemory();
        RuntimeTables = new RuntimeTablesMemory();
        WorkflowInstanceTable = RuntimeTables.WorkflowInstance as WorkflowInstanceTableMemory;
        FulcrumAssert.IsNotNull(WorkflowInstanceTable, CodeLocation.AsString());
        WorkflowEngineStorage = useStorage? new WorkflowEngineStorageMemory() : new NoWorkflowEngineStorage();
        if (useStorage)
        {
            WorkflowSummaryStore = WorkflowEngineStorage.WorkflowSummary as WorkflowSummaryStoreMemory;
            FulcrumAssert.IsNotNull(WorkflowSummaryStore, CodeLocation.AsString());
        }
        AsyncRequestMgmtMock = new AsyncRequestMgmtMock();
        var workflowCapabilities = new WorkflowCapabilities(ConfigurationTables, RuntimeTables, AsyncRequestMgmtMock, WorkflowEngineStorage);
        WorkflowContainer = new ProvokePersistenceFailuresContainer(workflowCapabilities);
        LogicMoq = new Mock<IProvokePersistenceFailures>();
        WorkflowContainer.AddImplementation(new Support.ProvokePersistenceFailuresImplementation(WorkflowContainer, LogicMoq.Object));
        WorkflowInstanceId = DataFixture.Create<Guid>();
        FulcrumApplication.Context.ExecutionId = WorkflowInstanceId.ToGuidString();
        FulcrumApplication.Context.ManagedAsynchronousRequestId = DataFixture.Create<Guid>().ToGuidString();
        WorkflowMgmtCapability = new WorkflowMgmtCapability(workflowCapabilities, RuntimeTables, ConfigurationTables);
    }

}