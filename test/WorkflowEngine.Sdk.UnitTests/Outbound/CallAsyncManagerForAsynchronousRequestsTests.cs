using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Outbound;
using Shouldly;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Outbound
{
    public class CallAsyncManagerForAsynchronousRequestsTests
    {
        private readonly ItemUnderTest _iut;
        private readonly Mock<IAsyncRequestMgmtCapability> _asyncManagerMock;

        public CallAsyncManagerForAsynchronousRequestsTests()
        {
            _asyncManagerMock = new Mock<IAsyncRequestMgmtCapability>();
            _iut = new ItemUnderTest(_asyncManagerMock.Object);
            WorkflowStatic.Context.ExecutionIsAsynchronous = true;
            var workflowInformationMock = new Mock<IWorkflowInformation>();
            var activityInstance = new ActivityInstance
            {
                AsyncRequestId = Guid.NewGuid().ToGuidString()
            };
            workflowInformationMock
                .Setup(ai => ai.GetActivityInstance(It.IsAny<string>()))
                .Returns(activityInstance);
            var activitionInformationMock = new Mock<IActivityInformation>();
            activitionInformationMock
                .SetupGet(ai => ai.Workflow)
                .Returns(workflowInformationMock.Object);
            var activity = new ActivityAction(activitionInformationMock.Object, (_, _) => Task.CompletedTask);
            WorkflowStatic.Context.LatestActivity = activity;
        }

        [Fact]
        public async Task Send_Given_ThrowsRequestPostponedException_ThrowsRequestPostponedWithNoInnerException()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/Persons/123");
            var requestResponseServiceMock = new Mock<IRequestResponseService>();
            var expectedException = new RequestPostponedException
            {
                TryAgain = true,
                TryAgainAfterMinimumTimeSpan = TimeSpan.FromSeconds(30)
            };
            requestResponseServiceMock
                .Setup(rr => rr.ReadResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);
            _asyncManagerMock
                .SetupGet(am => am.RequestResponse)
                .Returns(requestResponseServiceMock.Object);

            // Act && Assert
            var exception = await _iut.SendAsync(request, CancellationToken.None)
                .ShouldThrowAsync<RequestPostponedException>();
            exception.TryAgain.ShouldBe(true);
            exception.TryAgainAfterMinimumTimeSpan.ShouldBe(expectedException.TryAgainAfterMinimumTimeSpan);
            exception.InnerException.ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.NotTryAgainExceptions), MemberType = typeof(TestDataGenerator))]
        public async Task Send_Given_ThrowsNotTryAgainException_ThrowsRequestPostponedWithInnerException(Exception exceptionThrown)
        {
            // Arrange
            TimeSpan expectedTimeSpan;
            ;
            if (exceptionThrown is FulcrumException fulcrumException)
            {
                expectedTimeSpan = TimeSpan.FromSeconds(fulcrumException.RecommendedWaitTimeInSeconds);
            }
            else
            {
                expectedTimeSpan = TimeSpan.FromSeconds(60);
            }
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/Persons/123");
            var requestResponseServiceMock = new Mock<IRequestResponseService>();
            requestResponseServiceMock
                .Setup(rr => rr.ReadResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exceptionThrown);
            _asyncManagerMock
                .SetupGet(am => am.RequestResponse)
                .Returns(requestResponseServiceMock.Object);

            // Act && Assert
            var exception = await _iut.SendAsync(request, CancellationToken.None)
                .ShouldThrowAsync<RequestPostponedException>();
            exception.TryAgain.ShouldBe(true);
            exception.TryAgainAfterMinimumTimeSpan.ShouldBe(expectedTimeSpan);
            exception.InnerException.ShouldBeAssignableTo(exceptionThrown.GetType());
        }
    }

    public class ItemUnderTest : CallAsyncManagerForAsynchronousRequests
    {
        public ItemUnderTest(IAsyncRequestMgmtCapability asyncManager) : base(asyncManager)
        {
        }

        public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }

    public class TestDataGenerator
    {
        public static IEnumerable<object[]> NotTryAgainExceptions()
        {
            yield return new object[] { new FulcrumAssertionFailedException() };
            yield return new object[] { new FulcrumTryAgainException() };
            yield return new object[] { new FulcrumResourceException() };
            yield return new object[] { new OperationCanceledException() };
            yield return new object[] { new WebSocketException() };
        }
    }
}