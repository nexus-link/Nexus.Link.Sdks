using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.ActivityTypes;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Outbound;
using Shouldly;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Outbound;

public class CallAsyncManagerForAsynchronousRequestsTests
{
    private readonly ItemUnderTest _iut;
    private readonly Mock<IAsyncRequestMgmtCapability> _asyncManagerMock;

    public CallAsyncManagerForAsynchronousRequestsTests()
    {
        FulcrumApplicationHelper.UnitTestSetup(this.GetType().Name);
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
        var nexusAsyncSemaphore = new NexusAsyncSemaphore();
        workflowInformationMock
            .SetupGet(ai => ai.ReadResponsesSemaphore)
            .Returns(nexusAsyncSemaphore);
        workflowInformationMock
            .SetupSequence(ai => ai.HttpAsyncResponses)
            .Returns((IDictionary<string, HttpResponse>)null)
            .Returns(new ConcurrentDictionary<string, HttpResponse>())
            .Returns(new ConcurrentDictionary<string, HttpResponse>())
            .Returns(new ConcurrentDictionary<string, HttpResponse>())
            .Returns(new ConcurrentDictionary<string, HttpResponse>());
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
        var expectedException = new ActivityPostponedException(TimeSpan.FromSeconds(30));
        //requestResponseServiceMock
        //    .Setup(rr => rr.ReadResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //    .ThrowsAsync(expectedException);
        _asyncManagerMock
            .SetupGet(am => am.RequestResponse)
            .Returns(requestResponseServiceMock.Object);

        // Act && Assert
        var exception = await _iut.SendAsync(request, CancellationToken.None)
            .ShouldThrowAsync<ActivityWaitsForRequestException>();

        exception.TryAgainAfterMinimumTimeSpan.ShouldBeNull();
        exception.InnerException.ShouldBeNull();
    }

    [Theory]
    [MemberData(nameof(TestDataGenerator.NotTryAgainExceptions), MemberType = typeof(TestDataGenerator))]
    public async Task Send_Given_ThrowsNotTryAgainException_ThrowsRequestPostponedWithInnerException(Exception exceptionThrown)
    {
        // Arrange
        TimeSpan? expectedTimeSpan = null;
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/Persons/123");
        var requestResponseServiceMock = new Mock<IRequestResponseService>();
        //requestResponseServiceMock
        //    .Setup(rr => rr.ReadResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //    .ThrowsAsync(exceptionThrown);
        _asyncManagerMock
            .SetupGet(am => am.RequestResponse)
            .Returns(requestResponseServiceMock.Object);

        // Act && Assert
        var exception = await _iut.SendAsync(request, CancellationToken.None)
            .ShouldThrowAsync<RequestPostponedException>();

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