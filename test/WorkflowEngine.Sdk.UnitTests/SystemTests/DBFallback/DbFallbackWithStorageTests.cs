using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Shouldly;
using UnitTests.Support;
using WorkflowEngine.Sdk.UnitTests.SystemTests.DBFallback.Support;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.DBFallback;

public class DbFallbackWithStorageTests : Base
{

    public DbFallbackWithStorageTests(ITestOutputHelper testOutputHelper) :base(nameof(DbFallbackWithStorageTests), true)
    {
        FulcrumApplicationHelper.UnitTestSetup(this.GetType().Name);
        FulcrumApplication.Setup.SynchronousFastLogger = new XUnitFulcrumLogger(testOutputHelper);
    }

    [Fact]
    public async Task Execute_Given_AllGood_Gives_Returns()
    {
        // Arrange
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        var expectedResult = Guid.NewGuid().ToString();
        LogicMoq
            .Setup(l => l.ActionA())
            .Returns(expectedResult)
            .Verifiable();

        // Act
        var actualResult = await implementation.ExecuteAsync();

        // Assert
        LogicMoq.Verify();
        LogicMoq.VerifyNoOtherCalls();
        actualResult.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task Execute_Given_FirstTimeDbFailsStorageWorks_Gives_BlobStored()
    {
        // Arrange
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        var expectedResult = Guid.NewGuid().ToString();
        string workflowInstanceId = null;
        // Make DB fail, should result in blob
        LogicMoq
            .Setup(l => l.ActionA())
            .Callback(() =>
            {
                workflowInstanceId = WorkflowStatic.Context.WorkflowInstanceId;
                WorkflowInstanceTable.OnlyForTest_Update_AlwaysThrowThisException = new FulcrumResourceException();
            })
            .Returns(expectedResult)
            .Verifiable();

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        LogicMoq.VerifyNoOtherCalls();
        workflowInstanceId.ShouldNotBeNull();
        var path = IWorkflowSummaryServiceStorage.GetWorkflowSummaryPath(workflowInstanceId);
        var summary = await WorkflowEngineStorage.WorkflowSummary.ReadAsync(path);
        summary.ShouldNotBeNull();
    }

    [Fact]
    public async Task Execute_Given_FirstTimeDbFailsStorageWorks_Gives_SecondTimeReturns()
    {
        // Arrange
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        var expectedResult = Guid.NewGuid().ToString();
        string workflowInstanceId = null;
        // Make DB fail, should result in blob
        LogicMoq
            .Setup(l => l.ActionA())
            .Callback(() =>
            {
                workflowInstanceId = WorkflowStatic.Context.WorkflowInstanceId;
                WorkflowInstanceTable.OnlyForTest_Update_AlwaysThrowThisException = new FulcrumResourceException();
            })
            .Returns(expectedResult)
            .Verifiable();
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();
        // Make DB work again
        WorkflowInstanceTable.OnlyForTest_Update_AlwaysThrowThisException = null;
        implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);

        // Act
        var actualResult = await implementation.ExecuteAsync();

        // Assert
        LogicMoq.Verify();
        actualResult.ShouldBe(expectedResult);
        workflowInstanceId.ShouldNotBeNull();
        var path = IWorkflowSummaryServiceStorage.GetWorkflowSummaryPath(workflowInstanceId);
        var blob = await WorkflowEngineStorage.WorkflowSummary.ReadAsync(path);
        blob.ShouldBeNull();
    }

    [Fact]
    public async Task Execute_Given_FirstTimeDbFailsStorageFails_Gives_NoBlob()
    {
        // Arrange
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        var expectedResult = Guid.NewGuid().ToString();
        string workflowInstanceId = null;
        // Make DB and storage fail
        LogicMoq
            .Setup(l => l.ActionA())
            .Callback(() =>
            {
                workflowInstanceId = WorkflowStatic.Context.WorkflowInstanceId;
                WorkflowInstanceTable.OnlyForTest_Update_AlwaysThrowThisException = new FulcrumResourceException();
                WorkflowSummaryStore.OnlyForTest_Create_AlwaysThrowThisException = new FulcrumResourceException();
                WorkflowSummaryStore.OnlyForTest_Update_AlwaysThrowThisException = new FulcrumResourceException();
            })
            .Returns(expectedResult)
            .Verifiable();;

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();

        // Assert
        LogicMoq.Verify();
        workflowInstanceId.ShouldNotBeNull();
        var path = IWorkflowSummaryServiceStorage.GetWorkflowSummaryPath(workflowInstanceId);
        var blob = await WorkflowEngineStorage.WorkflowSummary.ReadAsync(path);
        blob.ShouldBeNull();
    }

    [Fact]
    public async Task Execute_Given_FirstTimeDbFailsStorageFails_Gives_SecondTimeWorkflowFails()
    {
        // Arrange
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        var expectedResult = Guid.NewGuid().ToString();
        string workflowInstanceId = null;
        // Make DB and storage fail
        LogicMoq
            .Setup(l => l.ActionA())
            .Callback(() =>
            {
                workflowInstanceId = WorkflowStatic.Context.WorkflowInstanceId;
                WorkflowInstanceTable.OnlyForTest_Update_AlwaysThrowThisException = new FulcrumResourceException();
                WorkflowSummaryStore.OnlyForTest_Create_AlwaysThrowThisException = new FulcrumResourceException();
                WorkflowSummaryStore.OnlyForTest_Update_AlwaysThrowThisException = new FulcrumResourceException();
            })
            .Returns(expectedResult)
            .Verifiable();
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();
        // Make DB and storage work again
        WorkflowInstanceTable.OnlyForTest_Update_AlwaysThrowThisException = null;
        WorkflowSummaryStore.OnlyForTest_Create_AlwaysThrowThisException = null;
        WorkflowSummaryStore.OnlyForTest_Update_AlwaysThrowThisException = null;
        implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<FulcrumCancelledException>();

        // Assert
        LogicMoq.Verify();
        workflowInstanceId.ShouldNotBeNull();
        var path = IWorkflowSummaryServiceStorage.GetWorkflowSummaryPath(workflowInstanceId);
        var blob = await WorkflowEngineStorage.WorkflowSummary.ReadAsync(path);
        blob.ShouldBeNull();
    }

    [Fact]
    public async Task Execute_Given_DbFailsStorageFails_Gives_WorkflowFails()
    {
        // Arrange
        var implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);
        var expectedResult = Guid.NewGuid().ToString();
        string workflowInstanceId = null;
        // Make DB and storage fail
        LogicMoq
            .Setup(l => l.ActionA())
            .Callback(() =>
            {
                workflowInstanceId = WorkflowStatic.Context.WorkflowInstanceId;
                WorkflowInstanceTable.OnlyForTest_Update_AlwaysThrowThisException = new FulcrumResourceException();
                WorkflowSummaryStore.OnlyForTest_Create_AlwaysThrowThisException = new FulcrumResourceException();
                WorkflowSummaryStore.OnlyForTest_Update_AlwaysThrowThisException = new FulcrumResourceException();
            })
            .Returns(expectedResult)
            .Verifiable();
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<RequestPostponedException>();
        // Make DB and storage work again
        WorkflowInstanceTable.OnlyForTest_Update_AlwaysThrowThisException = new FulcrumResourceException();
        WorkflowSummaryStore.OnlyForTest_Create_AlwaysThrowThisException = null;
        WorkflowSummaryStore.OnlyForTest_Update_AlwaysThrowThisException = null;
        implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<FulcrumCancelledException>();

        // Assert
        LogicMoq.Verify();
        workflowInstanceId.ShouldNotBeNull();
        var path = IWorkflowSummaryServiceStorage.GetWorkflowSummaryPath(workflowInstanceId);
        var blob = await WorkflowEngineStorage.WorkflowSummary.ReadAsync(path);
        blob.ShouldBeNull();
    }
}