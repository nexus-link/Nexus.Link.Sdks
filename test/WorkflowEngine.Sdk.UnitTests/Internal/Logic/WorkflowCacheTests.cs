using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;
using Shouldly;
using UnitTests.Support;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Logic;

public class WorkflowCacheTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    private WorkflowForm _form;
    private WorkflowVersion _version;
    private WorkflowInstance _instance;


    public WorkflowCacheTests(ITestOutputHelper testOutputHelper)
    {
        FulcrumApplicationHelper.UnitTestSetup(nameof(WorkflowCacheTests));
        FulcrumApplication.Setup.SynchronousFastLogger = new XUnitFulcrumLogger(testOutputHelper);
        FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Verbose;

        _testOutputHelper = testOutputHelper;

    }

    [Fact]
    public async Task Save_Given_NewValues_Gives_AfterSaveTrigger()
    {
        // Arrange
        var resetEvent = new ManualResetEvent(false);

        var workflowCapabilities = new WorkflowCapabilities(new ConfigurationTablesMemory(), new RuntimeTablesMemory(), new AsyncRequestMgmtMock(), new WorkflowEngineStorageMemory());
        await CreateWorkflowComponentsAsync(workflowCapabilities);

        const WorkflowStateEnum newState = WorkflowStateEnum.Success;
        const string newFormTitle = "new f title";
        const int newMinorVersion = 3;
        const string newInstanceTitle = "new i title";
        var newFinishedAt = DateTimeOffset.Now;

        IWorkflowInformation workflowInformation = new WorkflowInformationMock(_form, _version, _instance);
        workflowInformation.WorkflowOptions.AfterSaveAsync = (oldForm, oldVersion, oldInstance, newForm, newVersion, newInstance) =>
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
        var cache = new WorkflowCache(workflowInformation, workflowCapabilities);
        await cache.LoadAsync(default);

        // Act
        cache.Form.Title = newFormTitle;
        cache.Version.MinorVersion = newMinorVersion;
        cache.Instance.State = newState;
        cache.Instance.Title = newInstanceTitle;
        cache.Instance.FinishedAt = newFinishedAt;
        await cache.SaveWithFallbackAsync();

        // Assert
        resetEvent.WaitOne(100).ShouldBeTrue($"There is probably an error in {nameof(workflowInformation.WorkflowOptions.AfterSaveAsync)}. Look for an error message below.");
    }

    [Fact]
    public async Task Save_Given_DbFails_Gives_SavedToBlob()
    {
        // Arrange
        var storage = new WorkflowEngineStorageMemory();
        var configurationTables = new ConfigurationTablesMemory();
        var runtimeTables = new RuntimeTablesMemory();
        var asyncRequestMgmtCapabilityMock = new AsyncRequestMgmtMock();
        var workflowCapabilities = new WorkflowCapabilities(configurationTables, runtimeTables, asyncRequestMgmtCapabilityMock, storage);
        var workflowCapabilitiesMock = new Mock<IWorkflowEngineRequiredCapabilities>();
        workflowCapabilitiesMock
            .SetupGet(wc => wc.ConfigurationCapability)
            .Returns(workflowCapabilities.ConfigurationCapability);
        //workflowCapabilitiesMock
        //    .SetupGet(wc => wc.StateCapability)
        //    .Returns(workflowCapabilities.StateCapability);
        var workflowInstanceServiceMock = new Mock<IWorkflowInstanceService>();
        workflowInstanceServiceMock
            .Setup(wi =>
                wi.UpdateAsync(It.IsAny<string>(), It.IsAny<WorkflowInstance>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FulcrumResourceException());
        var activityInstanceServiceMock = new Mock<IActivityInstanceService>();
        var stateCapabilityMock = new Mock<IWorkflowStateCapability>();
        stateCapabilityMock
            .SetupGet(s => s.WorkflowInstance)
            .Returns(workflowInstanceServiceMock.Object);
        stateCapabilityMock
            .SetupGet(s => s.ActivityInstance)
            .Returns(activityInstanceServiceMock.Object);
        stateCapabilityMock
            .SetupGet(s => s.WorkflowSummary)
            .Returns(workflowCapabilities.StateCapability.WorkflowSummary);
        stateCapabilityMock
            .SetupGet(s => s.WorkflowSummaryStorage)
            .Returns(workflowCapabilities.StateCapability.WorkflowSummaryStorage);
        workflowCapabilitiesMock
            .SetupGet(wc => wc.StateCapability)
            .Returns(stateCapabilityMock.Object);

        var implementation = new TestWorkflowImplementation(workflowCapabilities);
        IWorkflowInformation workflowInformation = new WorkflowInformation(implementation);
        await CreateWorkflowComponentsAsync(workflowCapabilities, implementation.WorkflowContainer.WorkflowFormId, implementation.MajorVersion);

        workflowInformation.InstanceId = _instance.Id;
        var cache = new WorkflowCache(workflowInformation, workflowCapabilitiesMock.Object);
        await cache.LoadAsync(default);
        cache.Instance.Title = "Changed to force a save";

        // Act
        await cache.SaveWithFallbackAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        var workflowSummaryServiceStorage = new WorkflowSummaryServiceStorage(storage);
        var summary = await workflowSummaryServiceStorage.ReadBlobAsync(_instance.Id, _instance.StartedAt);
        summary.ShouldNotBeNull();
    }

    /// <summary>
    /// We have a contract for <see cref="WorkflowCache.SaveWithFallbackAsync"/>
    /// that it must call <see cref="WorkflowOptions.AfterSaveAsync"/>
    /// whenever it saves it values.
    /// </summary>
    [Fact]
    public async Task Load_Given_HasBlob_Gives_SavedToDbAndBlobRemoved()
    {
        // Arrange
        var expectedTitle = Guid.NewGuid().ToString();

        var storage = new WorkflowEngineStorageMemory();
        var configurationTables = new ConfigurationTablesMemory();
        var runtimeTables = new RuntimeTablesMemory();
        var asyncRequestMgmtCapabilityMock = new AsyncRequestMgmtMock();
        var workflowCapabilities = new WorkflowCapabilities(configurationTables, runtimeTables, asyncRequestMgmtCapabilityMock, storage);
        var implementation = new TestWorkflowImplementation(workflowCapabilities);
        IWorkflowInformation workflowInformation = new WorkflowInformation(implementation);
        await CreateWorkflowComponentsAsync(workflowCapabilities, implementation.WorkflowContainer.WorkflowFormId, implementation.MajorVersion);

        var workflowSummaryServiceDb =
            new WorkflowSummaryService(configurationTables, runtimeTables);
        var workflowSummaryServiceStorage =
            new WorkflowSummaryServiceStorage(storage);
        var workflowSummary = await workflowSummaryServiceDb.GetSummaryAsync(_instance.Id);
        // This change is only saved to blob, should eventually be saved to DB
        workflowSummary.Instance.Title = expectedTitle;
        await workflowSummaryServiceStorage.WriteBlobAsync(workflowSummary);

        workflowInformation.InstanceId = _instance.Id;
        var cache = new WorkflowCache(workflowInformation, workflowCapabilities);

        // Act
        await cache.LoadAsync(default);

        // Assert
        var summary = await workflowSummaryServiceStorage.ReadBlobAsync(_instance.Id, _instance.StartedAt);
        summary.ShouldBeNull();
        summary = await workflowSummaryServiceDb.GetSummaryAsync(_instance.Id);
        summary.ShouldNotBeNull();
        summary.Instance.Title.ShouldBe(expectedTitle);
    }

    private async Task CreateWorkflowComponentsAsync(WorkflowCapabilities workflowCapabilities, string formId = null, int? version = null)
    {
        _form = await workflowCapabilities.ConfigurationCapability.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(
            formId ?? Guid.NewGuid().ToString(), new WorkflowFormCreate
            {
                CapabilityName = "x",
                Title = "x"
            });
        _version = await workflowCapabilities.ConfigurationCapability.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(
            Guid.NewGuid().ToString(), new WorkflowVersionCreate
            {
                MajorVersion = version ?? 1,
                MinorVersion = 0,
                WorkflowFormId = _form.Id
            });
        _instance = await workflowCapabilities.StateCapability.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(
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