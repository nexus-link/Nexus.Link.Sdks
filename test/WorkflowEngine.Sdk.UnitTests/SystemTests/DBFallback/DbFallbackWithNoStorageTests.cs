using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.SystemTests.DBFallback.Support;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.SystemTests.DBFallback;

public class DbFallbackWithNoStorageTests : Base
{

    public DbFallbackWithNoStorageTests() :base(nameof(DbFallbackWithNoStorageTests), false)
    {
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
    public async Task Execute_Given_FirstTimeDbFailsNoStorage_Gives_FulcrumTryAgainException()
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
            })
            .Returns(expectedResult)
            .Verifiable();;

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<FulcrumTryAgainException>();

        // Assert
        LogicMoq.Verify();
        workflowInstanceId.ShouldNotBeNull();
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
            })
            .Returns(expectedResult)
            .Verifiable();
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<FulcrumTryAgainException>();
        // Make DB and storage work again
        WorkflowInstanceTable.OnlyForTest_Update_AlwaysThrowThisException = null;
        implementation = await WorkflowContainer.SelectImplementationAsync<string>(1, 1);

        // Act
        await implementation.ExecuteAsync()
            .ShouldThrowAsync<FulcrumCancelledException>();

        // Assert
        LogicMoq.Verify();
        workflowInstanceId.ShouldNotBeNull();
        var path = IWorkflowSummaryServiceStorage.GetWorkflowSummaryPath(workflowInstanceId);
    }
}