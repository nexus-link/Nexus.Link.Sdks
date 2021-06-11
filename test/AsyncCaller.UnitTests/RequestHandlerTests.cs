using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Nexus.Link.AsyncCaller.Sdk.Data.Models;
using Nexus.Link.AsyncCaller.Sdk.Dispatcher.Helpers;
using Nexus.Link.AsyncCaller.Sdk.Dispatcher.Logic;
using Nexus.Link.AsyncCaller.Sdk.Dispatcher.Models;
using Nexus.Link.AsyncCaller.Sdk.Storage.Memory.Queue;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Decoupling;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace AsyncCaller.Sdk.UnitTests
{
    [TestClass]
    public class RequestHandlerTests
    {
        private Mock<IHttpClient> _httpClientMock;
        private Mock<ILeverConfiguration> _leverConfigurationMock;

        private static readonly Tenant Tenant = new Tenant("nexus-sdk", "unit-tests");

        private bool _runBackgroundJob;
        private Action<RawRequestEnvelope> _queueAction;
        private readonly MemoryQueue _queue = MemoryQueue.Instance(RequestQueueHelper.DefaultQueueName);


        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(RequestHandlerTests));
            FulcrumApplication.Setup.SynchronousFastLogger = new ConsoleLogger();
            FulcrumApplication.Context.ClientTenant = Tenant;
        }

        private void RunBackgroundJobPoppingMessages()
        {
            ThreadHelper.FireAndForget(async () =>
            {
                while (_runBackgroundJob)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));

                    var message = _queue.GetOneMessageNoBlock();
                    if (message == null) continue;
                    if (_queueAction == null) continue;

                    var envelope = JsonConvert.DeserializeObject<RawRequestEnvelope>(message);
                    _queueAction(envelope);
                }
            });
        }

        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            _runBackgroundJob = true;

            RunBackgroundJobPoppingMessages();

            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.SetupGet(x => x.SimulateOutgoingCalls).Returns(true);

            _leverConfigurationMock = new Mock<ILeverConfiguration>();
            _leverConfigurationMock.Setup(x => x.Value<int?>(nameof(AnonymousSchema.SchemaVersion))).Returns(1);
            _leverConfigurationMock.Setup(x => x.MandatoryValue<string>("ConnectionString")).Returns(RequestQueueHelper.MemoryQueueConnectionString);
        }

        [TestCleanup]
        public void ClassCleanup()
        {
            _runBackgroundJob = false;
        }

        private async Task<RequestHandler> CreateRequestHandler(RequestEnvelope envelope, CancellationToken cancellationToken)
        {
            var rawEnvelope = await envelope.ToRawAsync();
            return new RequestHandler(_httpClientMock.Object, Tenant, _leverConfigurationMock.Object, rawEnvelope);
        }

        [TestMethod]
        public async Task ResponseCode_From_Failed_Request_Is_ApplicationJson_In_Callback()
        {
            // Arrange
            var resetEvent = new ManualResetEvent(false);

            _queueAction = async rawEnvelope =>
            {
                var envelope = await RequestEnvelope.FromRawAsync(rawEnvelope, TimeSpan.FromSeconds(1));
                Log.LogInformation($"Action on {rawEnvelope.RawRequest.Title}");

                // Process Out and Callback
                var handler = await CreateRequestHandler(envelope, CancellationToken.None);
                await handler.ProcessOneRequestAsync();

            };

            string callbackResponseString = null;
            const string expectedPayload = "Cannot work with that";
            const string expecteMediaType= "text/plain";

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback(async (HttpRequestMessage httpRequest, CancellationToken ct) =>
                {
                    Log.LogInformation($"Http client mock: {httpRequest.RequestUri}");
                    if (httpRequest.RequestUri.ToString().Contains("/callback"))
                    {
                        callbackResponseString = await httpRequest.Content.ReadAsStringAsync();
                        resetEvent.Set();
                    }
                })
                .ReturnsAsync((HttpRequestMessage httpRequest, CancellationToken ct) =>
                {
                    if (httpRequest.RequestUri.ToString().Contains("/out"))
                    {
                        // The CallOut will result in 400 Bad Request
                        return new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            Content = new StringContent(expectedPayload, Encoding.UTF8, expecteMediaType)
                        };
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            var request = new Request
            {
                CallOut = new HttpRequestMessage(HttpMethod.Post, "https://example.com/out"),
                CallBack = new HttpRequestMessage(HttpMethod.Post, "https://exmaple.com/callback")
            };
            var requestEnvelope = new RequestEnvelope(Tenant.Organization, Tenant.Environment, request, TimeSpan.FromMinutes(1));

            // Act
            await _queue.AddMessageAsync((await requestEnvelope.ToRawAsync()).Serialize());

            // Assert
            Assert.IsTrue(resetEvent.WaitOne(TimeSpan.FromMilliseconds(1000)), "This asynchronous request dodn't finish in time");

            Assert.IsNotNull(callbackResponseString, "We need the response content of the Callback request");

            var responseContent = JsonConvert.DeserializeObject<Nexus.Link.AsyncCaller.Sdk.Common.Models.ResponseContent>(callbackResponseString);
            Assert.IsNotNull(responseContent);
            Assert.AreEqual(HttpStatusCode.BadRequest, responseContent.StatusCode);
            Assert.AreEqual(expecteMediaType, responseContent.PayloadMediaType);
            Assert.AreEqual(expectedPayload, responseContent.Payload);
        }
    }
}
