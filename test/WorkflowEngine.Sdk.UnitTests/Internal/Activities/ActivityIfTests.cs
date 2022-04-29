using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Shouldly;
using WorkflowEngine.Sdk.UnitTests.TestSupport;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Internal.Activities
{
    public class ActivityIfTests
    {
        private readonly Mock<IActivityExecutor> _activityExecutorMock;
        private readonly ActivityInformationMock _activityInformationMock;

        public ActivityIfTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivityIfTests));
            _activityExecutorMock = new Mock<IActivityExecutor>();
            var workflowInformationMock = new WorkflowInformationMock(_activityExecutorMock.Object);
            _activityInformationMock = new ActivityInformationMock(workflowInformationMock);
            _activityExecutorMock.Setup(ae =>
                    ae.ExecuteWithoutReturnValueAsync(It.IsAny<InternalActivityMethodAsync>(), It.IsAny<CancellationToken>()))
                .Returns((InternalActivityMethodAsync m, CancellationToken ct) => m(ct));
            _activityExecutorMock.Setup(ae =>
                    ae.ExecuteWithReturnValueAsync(It.IsAny<InternalActivityMethodAsync<int>>(), It.IsAny<ActivityDefaultValueMethodAsync<int>>(), It.IsAny<CancellationToken>()))
                .Returns((InternalActivityMethodAsync<int> m, ActivityDefaultValueMethodAsync<int> d, CancellationToken ct) => m(ct));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteWithNoResult_Given_Condition_Gives_CallsThenOrElse(bool condition)
        {
            // Arrange
            var thenExecuted = false;
            var elseExecuted = false;
            var activity = new ActivityIf(_activityInformationMock, (a, ct) => Task.FromResult(condition));
            activity.Then((a, ct) => {
                thenExecuted = true;
                return Task.CompletedTask;
            });
            activity.Else((a, ct) => {
                elseExecuted = true;
                return Task.CompletedTask;
            });

            // Act
            await activity.ExecuteAsync();

            // Assert
            thenExecuted.ShouldBe(condition);
            elseExecuted.ShouldBe(!condition);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteWithNoResult_Given_NoElse_Gives_CallsThenIfTrue(bool condition)
        {
            // Arrange
            var thenExecuted = false;
            var activity = new ActivityIf(_activityInformationMock, (a, ct) => Task.FromResult(condition));
            activity.Then((a, ct) => {
                thenExecuted = true;
                return Task.CompletedTask;
            });

            // Act
            await activity.ExecuteAsync();

            // Assert
            thenExecuted.ShouldBe(condition);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteWithNoResult_Given_NoThen_Gives_CallsElseIfFalse(bool condition)
        {
            // Arrange
            var elseExecuted = false;
            var activity = new ActivityIf(_activityInformationMock, (a, ct) => Task.FromResult(condition));
            activity.Else((a, ct) => {
                elseExecuted = true;
                return Task.CompletedTask;
            });

            // Act
            await activity.ExecuteAsync();

            // Assert
            elseExecuted.ShouldBe(!condition);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteWithResult_Given_Condition_Gives_CallsThenOrElse(bool condition)
        {
            // Arrange
            var thenExecuted = false;
            var elseExecuted = false;
            const int thenResult = 1;
            const int elseResult = 2;
            var activity = new ActivityIf<int>(_activityInformationMock, null, (a, ct) => Task.FromResult(condition));
            activity.Then((a, ct) => {
                thenExecuted = true;
                return Task.FromResult(thenResult);
            });
            activity.Else((a, ct) => {
                elseExecuted = true;
                return Task.FromResult(elseResult);
            });

            // Act
            var result = await activity.ExecuteAsync();

            // Assert
            thenExecuted.ShouldBe(condition);
            elseExecuted.ShouldBe(!condition);
            result.ShouldBe(condition ? thenResult : elseResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteWithResult_Given_NoElse_Gives_Throws(bool condition)
        {
            // Arrange
            var activity = new ActivityIf<int>(_activityInformationMock, null, (a, ct) => Task.FromResult(condition));
            activity.Then((a, ct) => Task.FromResult(1));

            // Act & Assert
            // TODO: Why doesn't this work?
            //await activity.ExecuteAsync()
            //    .ShouldThrowAsync<FulcrumContractException>();
            try
            {
                await activity.ExecuteAsync();
                FulcrumAssert.Fail(CodeLocation.AsString());
            }
            catch (FulcrumContractException)
            {
                // OK
            }
            catch (Exception)
            {
                FulcrumAssert.Fail(CodeLocation.AsString());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteWithResult_Given_NoThen_Gives_Throws(bool condition)
        {
            // Arrange
            var activity = new ActivityIf<int>(_activityInformationMock, null, (a, ct) => Task.FromResult(condition));
            activity.Else((a, ct) => Task.FromResult(1));

            // Act & Assert
            // TODO: Why doesn't this work?
            //await activity.ExecuteAsync()
            //    .ShouldThrowAsync<FulcrumContractException>();
            try
            {
                await activity.ExecuteAsync();
                FulcrumAssert.Fail(CodeLocation.AsString());
            }
            catch (FulcrumContractException)
            {
                // OK
            }
            catch (Exception)
            {
                FulcrumAssert.Fail(CodeLocation.AsString());
            }
        }
    }
}

