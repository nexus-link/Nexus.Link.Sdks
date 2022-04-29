using System;
using System.Collections.Generic;
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
    public class ActivitySwitchTests
    {
        private readonly Mock<IActivityExecutor> _activityExecutorMock;
        private readonly ActivityInformationMock _activityInformationMock;

        public ActivitySwitchTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ActivitySwitchTests));
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
        [InlineData(1, false)]
        [InlineData(2, false)]
        [InlineData(3, true)]
        public async Task ExecuteWithNoResult_Given_SwitchValue_Gives_CallsCaseOrDefault(int switchValue, bool expectDefaultExecuted)
        {
            // Arrange
            var caseExecuted = new Dictionary<int, bool>();
            var defaultExecuted = false;
            caseExecuted[switchValue] = false;
            var activity = new ActivitySwitch<int>(_activityInformationMock, (a, ct) => Task.FromResult(switchValue));
            activity.Case(1, (a, ct) =>
            {
                caseExecuted[1] = true;
                return Task.CompletedTask;
            });
            activity.Case(2, (a, ct) =>
            {
                caseExecuted[2] = true;
                return Task.CompletedTask;
            });
            activity.Default((a, ct) =>
            {
                defaultExecuted = true;
                return Task.CompletedTask;
            });

            // Act
            await activity.ExecuteAsync();

            // Assert
            foreach (var pair in caseExecuted)
            {
                if (switchValue.Equals(pair.Key) && !expectDefaultExecuted)
                {
                    pair.Value.ShouldBe(true);
                }
                else
                {
                    pair.Value.ShouldBe(false);
                }
            }
            defaultExecuted.ShouldBe(expectDefaultExecuted);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        public async Task ExecuteWithNoResult_Given_NoDefault_Gives_CallsNone(int switchValue)
        {
            // Arrange
            var caseExecuted = new Dictionary<int, bool>();
            caseExecuted[switchValue] = false;
            var activity = new ActivitySwitch<int>(_activityInformationMock, (a, ct) => Task.FromResult(switchValue));
            activity.Case(1, (a, ct) =>
            {
                caseExecuted[1] = true;
                return Task.CompletedTask;
            });
            activity.Case(2, (a, ct) =>
            {
                caseExecuted[2] = true;
                return Task.CompletedTask;
            });

            // Act
            await activity.ExecuteAsync();

            // Assert
            foreach (var pair in caseExecuted)
            {
                pair.Value.ShouldBe(false);
            }
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(2, false)]
        [InlineData(3, true)]
        public async Task ExecuteWithResult_Given_SwitchValue_Gives_CallsCaseOrDefault(int switchValue, bool expectDefaultExecuted)
        {
            // Arrange
            var caseExecuted = new Dictionary<int, bool>();
            var defaultExecuted = false;
            caseExecuted[switchValue] = false;
            var activity = new ActivitySwitch<int, int>(_activityInformationMock, null, (a, ct) => Task.FromResult(switchValue));
            activity.Case(1, (a, ct) =>
            {
                caseExecuted[1] = true;
                return Task.FromResult(11);
            });
            activity.Case(2, (a, ct) =>
            {
                caseExecuted[2] = true;
                return Task.FromResult(12);
            });
            activity.Default((a, ct) =>
            {
                defaultExecuted = true;
                return Task.FromResult(99);
            });

            // Act
            var value = await activity.ExecuteAsync();

            // Assert
            foreach (var pair in caseExecuted)
            {
                if (switchValue.Equals(pair.Key) && !expectDefaultExecuted)
                {
                    pair.Value.ShouldBe(true);
                    value.ShouldBe(pair.Key+10);
                }
                else
                {
                    pair.Value.ShouldBe(false);
                }
            }
            defaultExecuted.ShouldBe(expectDefaultExecuted);
            if (defaultExecuted) value.ShouldBe(99);
        }

        [Fact]
        public async Task ExecuteWithResult_Given_NoDefault_Gives_Throws()
        {
            // Arrange
            var activity = new ActivitySwitch<int, int>(_activityInformationMock, null, (a, ct) => Task.FromResult(3));
            activity.Case(1, (a, ct) => Task.FromResult(11));
            activity.Case(2, (a, ct) => Task.FromResult(12));

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

