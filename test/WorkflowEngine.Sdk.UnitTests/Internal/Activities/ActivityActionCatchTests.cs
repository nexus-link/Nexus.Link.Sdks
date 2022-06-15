using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities
{
    public class ActivityActionCatchTests : ActivityTestsBase
    {
        public ActivityActionCatchTests() : base(nameof(ActivityActionCatchTests))
        {
        }

        #region No return value
        [Fact]
        public async Task Execute_Given_Normal_Gives_ActivityExecutorActivated()
        {
            // Arrange
            var activity = new ActivityAction(_activityInformationMock, (_, _) => throw new ActivityFailedException(ActivityExceptionCategoryEnum.BusinessError, "fail", "fail"));

            // Act
            await activity
                .CatchAll((_, _, _) => Task.CompletedTask)
                .ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(e => e.ExecuteWithoutReturnValueAsync(activity.ActionAsync, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TryCatch_Given_Success_Gives_NoCatch()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            var activity = new ActivityAction(_activityInformationMock, (_, _) => Task.CompletedTask);
            activity.CatchAll((_, _, _) => Task.CompletedTask);

            // Act
            await activity.ActionAsync();

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(0);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Try");
            logicExecutor.ExecuteWithoutReturnValueCounter["Try"].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(1);
        }

        [Theory]
        [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
        [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
        [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
        [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
        public async Task TryCatch_Given_FailAndCatchAll_Gives_Catch(ActivityExceptionCategoryEnum category)
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            var activity = new ActivityAction(_activityInformationMock, (_, _) => Task.FromException(new ActivityFailedException(category, "Fail", "Fail")));
            activity.CatchAll((_, _, _) => Task.CompletedTask);

            // Act
            await activity.ActionAsync();

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(0);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Try");
            logicExecutor.ExecuteWithoutReturnValueCounter["Try"].ShouldBe(1);
            var key = "Catch all";
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey(key);
            logicExecutor.ExecuteWithoutReturnValueCounter[key].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(2);
        }

        [Fact]
        public async Task TryCatch_Given_FailAndNoMatchButCatchAll_Gives_CatchAll()
        {
            // Arrange
            var category1 = ActivityExceptionCategoryEnum.BusinessError;
            var category2 = ActivityExceptionCategoryEnum.TechnicalError;
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            var activity = new ActivityAction(_activityInformationMock, (_, _) => Task.FromException(new ActivityFailedException(category1, "Fail", "Fail")));
            activity.Catch(category2, (_, _, _) => Task.CompletedTask);
            activity.CatchAll((_, _, _) => Task.CompletedTask);

            // Act
            await activity.ActionAsync();

            // Assert
            logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(0);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Try");
            logicExecutor.ExecuteWithoutReturnValueCounter["Try"].ShouldBe(1);
            var key = "Catch all";
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey(key);
            logicExecutor.ExecuteWithoutReturnValueCounter[key].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(2);
        }

        [Theory]
        [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
        [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
        [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
        [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
        public async Task TryCatch_Given_FailWithCatchCategory_Gives_CatchCategory(ActivityExceptionCategoryEnum expectedCategory)
        {
            // Arrange
            var expectedCaughtCategory = expectedCategory.ToString();
            string actualCaughtCategory = null;
            ActivityExceptionCategoryEnum? actualCategory = null;
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            var activity = new ActivityAction(_activityInformationMock, (_, _) => Task.FromException(new ActivityFailedException(expectedCategory, "Fail", "Fail")));
            activity.Catch(expectedCategory, (_, exception, _) =>
            {
                actualCaughtCategory = expectedCategory.ToString();
                actualCategory = exception.ExceptionCategory;
                return Task.CompletedTask;
            });
            activity.CatchAll((_, exception, _) =>
            {
                actualCaughtCategory = "default";
                actualCategory = exception.ExceptionCategory;
                return Task.CompletedTask;
            });

            // Act
            await activity.ActionAsync();

            // Assert
            actualCategory.ShouldBe(expectedCategory);
            actualCaughtCategory.ShouldBe(expectedCaughtCategory);
            logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(0);
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey("Try");
            logicExecutor.ExecuteWithoutReturnValueCounter["Try"].ShouldBe(1);
            var key = $"Catch {expectedCaughtCategory}";
            logicExecutor.ExecuteWithoutReturnValueCounter.ShouldContainKey(key);
            logicExecutor.ExecuteWithoutReturnValueCounter[key].ShouldBe(1);
            logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(2);
        }
        #endregion

        #region Return value
        [Fact]
        public async Task RV_Execute_Given_Normal_Gives_ActivityExecutorActivated()
        {
            // Arrange
            var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => Task.FromResult(1));
            activity.CatchAll((_, _, _) => Task.FromResult(2));

            // Act
            await activity.ExecuteAsync();

            // Assert
            _activityExecutorMock.Verify(e => e.ExecuteWithReturnValueAsync(activity.ActionAsync, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RV_TryCatch_Given_Success_Gives_NoCatch()
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => Task.FromResult(1));
            activity.CatchAll((_, _, _) => Task.FromResult(1));

            // Act
            await activity.ActionAsync();

            // Assert
            logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Try");
            logicExecutor.ExecuteWithReturnValueCounter["Try"].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(1);
        }

        [Theory]
        [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
        [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
        [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
        [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
        public async Task RV_TryCatch_Given_FailAndCatchAll_Gives_Catch(ActivityExceptionCategoryEnum category)
        {
            // Arrange
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => throw new ActivityFailedException(category, "Fail", "Fail"));
            activity.CatchAll((_, _, _) => Task.FromResult(1));

            // Act
            await activity.ActionAsync();

            // Assert
            logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Try");
            logicExecutor.ExecuteWithReturnValueCounter["Try"].ShouldBe(1);
            var key = "Catch default";
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey(key);
            logicExecutor.ExecuteWithReturnValueCounter[key].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(2);
        }

        [Fact]
        public async Task RV_TryCatch_Given_FailAndNoMatchButCatchAll_Gives_CatchAll()
        {
            // Arrange
            var category1 = ActivityExceptionCategoryEnum.BusinessError;
            var category2 = ActivityExceptionCategoryEnum.TechnicalError;
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => throw new ActivityFailedException(category1, "Fail", "Fail"));
            activity.Catch(category2, (_, _, _) => Task.FromResult(1));
            activity.CatchAll((_, _, _) => Task.FromResult(1));

            // Act
            await activity.ActionAsync();

            // Assert
            logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Try");
            logicExecutor.ExecuteWithReturnValueCounter["Try"].ShouldBe(1);
            var key = "Catch default";
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey(key);
            logicExecutor.ExecuteWithReturnValueCounter[key].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(2);
        }

        [Theory]
        [InlineData(ActivityExceptionCategoryEnum.BusinessError)]
        [InlineData(ActivityExceptionCategoryEnum.TechnicalError)]
        [InlineData(ActivityExceptionCategoryEnum.MaxTimeReachedError)]
        [InlineData(ActivityExceptionCategoryEnum.WorkflowCapabilityError)]
        [InlineData(ActivityExceptionCategoryEnum.WorkflowImplementationError)]
        public async Task RV_TryCatch_Given_FailWithCatchCategory_Gives_CatchCategory(ActivityExceptionCategoryEnum expectedCategory)
        {
            // Arrange
            var expectedCaughtCategory = expectedCategory.ToString();
            string actualCaughtCategory = null;
            ActivityExceptionCategoryEnum? actualCategory = null;
            var logicExecutor = new LogicExecutorMock();
            _workflowInformationMock.LogicExecutor = logicExecutor;
            var activity = new ActivityAction<int>(_activityInformationMock, null, (_, _) => throw new ActivityFailedException(expectedCategory, "Fail", "Fail"));
            activity.Catch(expectedCategory, (_, exception, _) =>
            {
                actualCaughtCategory = expectedCategory.ToString();
                actualCategory = exception.ExceptionCategory;
                return Task.FromResult((int)expectedCategory);
            });
            activity.CatchAll((_, exception, _) =>
            {
                actualCaughtCategory = "default";
                actualCategory = exception.ExceptionCategory;
                return Task.FromResult((int)expectedCategory);
            });

            // Act
            await activity.ActionAsync();

            // Assert
            actualCategory.ShouldBe(expectedCategory);
            actualCaughtCategory.ShouldBe(expectedCaughtCategory);
            logicExecutor.ExecuteWithoutReturnValueCounter.Count.ShouldBe(0);
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey("Try");
            logicExecutor.ExecuteWithReturnValueCounter["Try"].ShouldBe(1);
            var key = $"Catch {expectedCaughtCategory}";
            logicExecutor.ExecuteWithReturnValueCounter.ShouldContainKey(key);
            logicExecutor.ExecuteWithReturnValueCounter[key].ShouldBe(1);
            logicExecutor.ExecuteWithReturnValueCounter.Count.ShouldBe(2);
        }

        #endregion
    }
}

