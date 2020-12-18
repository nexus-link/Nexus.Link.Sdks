using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.AsyncCaller.Sdk;
using Nexus.Link.AsyncCaller.Sdk.Data.Models;
using Nexus.Link.AsyncCaller.Sdk.Data.Queues;
using Nexus.Link.AsyncCaller.Sdk.Storage.Queue;
using Nexus.Link.Libraries.Core.Application;

namespace AsyncCaller.Sdk.UnitTests
{
    [TestClass]
    public class AyncCallTests
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AyncCallTests));
        }

        [TestMethod]
        public async Task MinimalAsyncCall()
        {
            RawRequest foundRawRequest = null;
            var asyncCallerMock = new Mock<IAsyncCaller>();
            var asyncCall = new AsyncCall(asyncCallerMock.Object, HttpMethod.Get, new Uri("http://ignore"));
            asyncCallerMock
                .Setup(ac => ac.ExecuteAsync(It.IsAny<RawRequest>()))
                .Callback((RawRequest r) => foundRawRequest = r)
                .ReturnsAsync("ignore");
            await asyncCall.ExecuteAsync();
            Assert.IsNotNull(foundRawRequest);
            Assert.IsNull(foundRawRequest.CallBack);
            Assert.IsNull(foundRawRequest.CallBackUriScheme);
            Assert.AreEqual("http", foundRawRequest.CallOutUriScheme);
            Assert.IsNull(foundRawRequest.Context);
            var expectedTitle = $"GET {asyncCall.CallOut.RequestUri} ({foundRawRequest.Id})";
            Assert.AreEqual(expectedTitle, foundRawRequest.Title);
            Assert.IsNull(foundRawRequest.Priority);
        }

        [TestMethod]
        public async Task PriorityAsyncCall()
        {
            RawRequest foundRawRequest = null;
            var asyncCallerMock = new Mock<IAsyncCaller>();
            var asyncCall = new AsyncCall(asyncCallerMock.Object, HttpMethod.Get, new Uri("http://ignore"))
                .SetPriority(2);
            asyncCallerMock
                .Setup(ac => ac.ExecuteAsync(It.IsAny<RawRequest>()))
                .Callback((RawRequest r) => foundRawRequest = r)
                .ReturnsAsync("ignore");
            await asyncCall.ExecuteAsync();
            Assert.IsNotNull(foundRawRequest);
            Assert.AreEqual(2, foundRawRequest.Priority);
        }

        [TestMethod]
        public async Task MaxDelayTimeAccordingToAzureStorageQueue()
        {
            // Arrange
            TimeSpan? actualTimeSpan = null;
            var queueMock = new Mock<IQueue>();
            queueMock
                .Setup(ac => ac.AddMessageAsync(It.IsAny<string>(), It.Is<TimeSpan?>(span => span != null)))
                .Callback((string s, TimeSpan? ts) => actualTimeSpan = ts)
                .Returns(Task.CompletedTask);
            queueMock
                .Setup(ac => ac.MaybeCreateAndConnect(It.IsAny<string>()));

            var requestQueue = new RequestQueue(queueMock.Object, nameof(AsyncCall));
            var envelope = new RawRequestEnvelope
            {
                Attempts = 1000, // Big number to ensure that we get a long delay
                RawRequest = new RawRequest{Id = null}
            };

            // Act
            await requestQueue.RequeueAsync(envelope, null);

            // Assert
            Assert.IsNotNull(actualTimeSpan);
            var expectedTimeSpan = TimeSpan.FromDays(7) - TimeSpan.FromSeconds(1);
            Assert.AreEqual(expectedTimeSpan, actualTimeSpan);
        }
    }
}
