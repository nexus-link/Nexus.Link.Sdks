using System;
using AutoFixture;
using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Components.WorkflowMgmt.Abstract;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using WorkflowEngine.Sdk.UnitTests.TestSupport;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.OneAction.Support;

public abstract class Base
{
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
        AsyncRequestMgmtMock = new AsyncRequestMgmtMock();
        var workflowCapabilities = new WorkflowCapabilities(ConfigurationTables, RuntimeTables, AsyncRequestMgmtMock);
        WorkflowContainer = new MyWorkflowContainer(workflowCapabilities);
        LogicMoq = new Mock<IMyLogic>();
        WorkflowContainer.AddImplementation(new MyWorkflowImplementation(WorkflowContainer, LogicMoq.Object));
        WorkflowInstanceId = DataFixture.Create<Guid>();
        FulcrumApplication.Context.ExecutionId = WorkflowInstanceId.ToGuidString();
        FulcrumApplication.Context.ManagedAsynchronousRequestId = DataFixture.Create<Guid>().ToGuidString();
        WorkflowMgmtCapability = new WorkflowMgmtCapability(workflowCapabilities);
    }

}