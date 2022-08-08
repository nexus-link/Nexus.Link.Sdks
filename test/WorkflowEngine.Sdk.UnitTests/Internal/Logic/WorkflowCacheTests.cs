using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic;

public class WorkflowCacheTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly WorkflowCapabilities _workflowCapabilities;
    private readonly WorkflowOptions _workflowOptions;

    public WorkflowCacheTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _workflowOptions = new WorkflowOptions();
        _workflowCapabilities = new WorkflowCapabilities(new ConfigurationTablesMemory(), new RuntimeTablesMemory(), new AsyncRequestMgmtMock(), _workflowOptions);

    }
    // TODO: "Contract" for SaveAsync to call a method with new and old when changed

    [Fact]
    public async Task TODO()
    {
        // Arrange
        var resetEvent = new ManualResetEvent(false);
        _workflowOptions.AfterSaveAsync = (form, version, instance, newForm, newVersion, newInstance) =>
        {
            // TODO: Check that properties have changed
            resetEvent.Set();
            
            return  Task.CompletedTask;
        };

        IWorkflowInformation workflowInformation = new WorkflowInformationMock(null, null);
        var cache = new WorkflowCache(workflowInformation, _workflowCapabilities);
        await cache.LoadAsync(default);

        // Act
        cache.Instance.State = WorkflowStateEnum.Success;
        await cache.SaveAsync();

        // Assert
        resetEvent.WaitOne(100).ShouldBeTrue();
    }
}