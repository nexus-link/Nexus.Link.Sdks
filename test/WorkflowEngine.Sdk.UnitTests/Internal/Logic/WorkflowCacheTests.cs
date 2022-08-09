using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Application;
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

    private WorkflowForm _form;
    private WorkflowVersion _version;
    private WorkflowInstance _instance;
    

    public WorkflowCacheTests(ITestOutputHelper testOutputHelper)
    {
        FulcrumApplicationHelper.UnitTestSetup(nameof(WorkflowCacheTests));

        _testOutputHelper = testOutputHelper;
        _workflowOptions = new WorkflowOptions();
        _workflowCapabilities = new WorkflowCapabilities(new ConfigurationTablesMemory(), new RuntimeTablesMemory(), new AsyncRequestMgmtMock(), _workflowOptions);

    }

    /// <summary>
    /// We have a contract for <see cref="WorkflowCache.SaveAsync"/>
    /// that it must call <see cref="WorkflowOptions.AfterSaveAsync"/>
    /// whenever it saves it values.
    /// </summary>
    [Fact]
    public async Task Given_New_Values_Gives_AfterSave_Trigger()
    {
        // Arrange
        var resetEvent = new ManualResetEvent(false);
        
        await CreateWorkflowComponentsAsync();

        const WorkflowStateEnum newState = WorkflowStateEnum.Success;
        const string newFormTitle = "new f title";
        const int newMinorVersion = 3;
        const string newInstanceTitle = "new i title";
        var newFinishedAt = DateTimeOffset.Now;

        _workflowOptions.AfterSaveAsync = (oldForm, oldVersion, oldInstance, newForm, newVersion, newInstance) =>
        {
            try
            {
                oldForm.Title.ShouldBe(_form.Title);
                newForm.Title.ShouldBe(newFormTitle);

                oldVersion.MinorVersion.ShouldBe(_version.MinorVersion);
                newVersion.MinorVersion.ShouldBe(newMinorVersion);

                oldInstance.State.ShouldBe(_instance.State);
                newInstance.State.ShouldBe(newState);
                oldInstance.Title.ShouldBe(_instance.Title);
                newInstance.Title.ShouldBe(newInstanceTitle);
                oldInstance.FinishedAt.ShouldBe(_instance.FinishedAt);
                newInstance.FinishedAt.ShouldBe(newFinishedAt);

                resetEvent.Set();
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.Message);
            }
            return Task.CompletedTask;
        };
        
        IWorkflowInformation workflowInformation = new WorkflowInformationMock(_form, _version, _instance);
        var cache = new WorkflowCache(workflowInformation, _workflowCapabilities);
        await cache.LoadAsync(default);

        // Act
        cache.Form.Title = newFormTitle;
        cache.Version.MinorVersion = newMinorVersion;
        cache.Instance.State = newState;
        cache.Instance.Title = newInstanceTitle;
        cache.Instance.FinishedAt = newFinishedAt;
        await cache.SaveAsync();

        // Assert
        resetEvent.WaitOne(100).ShouldBeTrue($"There is probably an error in {nameof(_workflowOptions.AfterSaveAsync)}. Look for an error message below.");
    }

    private async Task CreateWorkflowComponentsAsync()
    {
        _form = await _workflowCapabilities.ConfigurationCapability.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(
            Guid.NewGuid().ToString(), new WorkflowFormCreate
            {
                CapabilityName = "x",
                Title = "x"
            });
        _version = await _workflowCapabilities.ConfigurationCapability.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(
            Guid.NewGuid().ToString(), new WorkflowVersionCreate
            {
                MajorVersion = 1,
                MinorVersion = 0,
                WorkflowFormId = _form.Id
            });
        _instance = await _workflowCapabilities.StateCapability.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(
            Guid.NewGuid().ToString(), new WorkflowInstanceCreate
            {
                State = WorkflowStateEnum.Executing,
                StartedAt = DateTimeOffset.Now,
                Title = "x",
                WorkflowVersionId = _version.Id,
                InitialVersion = "1.0"
            });
    }
}