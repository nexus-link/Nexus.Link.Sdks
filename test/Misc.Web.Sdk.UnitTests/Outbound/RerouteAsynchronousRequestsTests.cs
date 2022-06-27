using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Misc.Web.Sdk.Outbound;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Shouldly;

#pragma warning disable CS0618

namespace Misc.Web.Sdk.UnitTests.Outbound
{
    [TestClass]
    public class RerouteAsynchronousRequestsTests
    {
        private NexusLinkHandlerOptions _options;
        private NexusLinkHandler _handler;
        private Mock<IRequestService> _requestServiceMoq;
        private Mock<IRequestResponseService> _requestResponseServiceMoq;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(RerouteAsynchronousRequestsTests).FullName);
            _options = new NexusLinkHandlerOptions();
            _handler = new NexusLinkHandler(_options);
            _requestServiceMoq = new Mock<IRequestService>();
            _requestResponseServiceMoq = new Mock<IRequestResponseService>();
            var asyncRequestMgmtCapabilityMoq = new Mock<IAsyncRequestMgmtCapability>();
            asyncRequestMgmtCapabilityMoq.SetupGet(c => c.Request).Returns(_requestServiceMoq.Object);
            asyncRequestMgmtCapabilityMoq.SetupGet(c => c.RequestResponse).Returns(_requestResponseServiceMoq.Object);
            _options.Features.RerouteAsynchronousRequests.Enabled = true;
            _options.Features.RerouteAsynchronousRequests.AsyncRequestMgmtCapability = asyncRequestMgmtCapabilityMoq.Object;
        }

        [TestMethod]
        public async Task SendAsync_Given_Not_ExecutionIsAsynchronous_Gives_NoCalls()
        {
            //
            // Arrange
            //
            FulcrumApplication.Context.ExecutionIsAsynchronous = false;

            // Simulate a http request
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/request");

            // Act
            var response = await _handler.TestSendAsync(request, (req, cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            //
            // Assert
            //
            response.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            _requestServiceMoq.Verify(service => service.CreateAsync(It.IsAny<HttpRequestCreate>(), It.IsAny<CancellationToken>()), Times.Never);
            _requestResponseServiceMoq.Verify(service => service.ReadResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task SendAsync_Given_NoAsyncRequestId_Gives_CreateRequest()
        {
            //
            // Arrange
            //
            FulcrumApplication.Context.ExecutionIsAsynchronous = true;
            var expectedAsyncRequestId = Guid.NewGuid().ToGuidString();
            FulcrumApplication.Context.AsyncRequestId = null;

            // Simulate a http request
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/request");
            _requestServiceMoq
                .Setup(service => service.CreateAsync(It.IsAny<HttpRequestCreate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAsyncRequestId);

            // Act
            var exception = await _handler.TestSendAsync(request, (req, cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)))
                .ShouldThrowAsync<RequestPostponedException>();

            //
            // Assert
            //
            exception.WaitingForRequestIds.Count.ShouldBe(1);
            exception.WaitingForRequestIds.FirstOrDefault().ShouldBe(expectedAsyncRequestId);
            _requestServiceMoq.Verify(service => service.CreateAsync(It.IsAny<HttpRequestCreate>(), It.IsAny<CancellationToken>()), Times.Once);
            _requestResponseServiceMoq.Verify(service => service.ReadResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task SendAsync_Given_AsyncRequestId_Gives_GetResponse()
        {
            //
            // Arrange
            //
            FulcrumApplication.Context.ExecutionIsAsynchronous = true;
            var expectedAsyncRequestId = Guid.NewGuid().ToGuidString();
            FulcrumApplication.Context.AsyncRequestId = expectedAsyncRequestId;

            // Simulate a http request
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/request");

            // Act
            var exception = await _handler.TestSendAsync(request, (req, cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)))
                .ShouldThrowAsync<RequestPostponedException>();

            //
            // Assert
            //
            exception.WaitingForRequestIds.Count.ShouldBe(1);
            exception.WaitingForRequestIds.FirstOrDefault().ShouldBe(expectedAsyncRequestId);
            _requestServiceMoq.Verify(service => service.CreateAsync(It.IsAny<HttpRequestCreate>(), It.IsAny<CancellationToken>()), Times.Never);
            _requestResponseServiceMoq.Verify(service => service.ReadResponseAsync(expectedAsyncRequestId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
