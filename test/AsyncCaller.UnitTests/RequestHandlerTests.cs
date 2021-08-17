using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
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
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.ServiceAuthentication;

namespace AsyncCaller.Sdk.UnitTests
{
    [TestClass]
    public class RequestHandlerTests
    {
        private Mock<IHttpClient> _httpClientMock;
        private Mock<ILeverConfiguration> _leverConfigurationMock;
        private Mock<IServiceAuthenticationHelper> _serviceAuthenticationHelperMock;

        private static readonly Tenant Tenant = new Tenant("nexus-sdk", "unit-tests");

        private bool _runBackgroundJob;
        private Action<RawRequestEnvelope> _queueAction;
        private readonly MemoryQueue _queue = MemoryQueue.Instance(RequestQueueHelper.DefaultQueueName);
        private static readonly AuthenticationSettings AuthenticationSettings = new AuthenticationSettings
        {
            Methods = new List<ClientAuthorizationSettings> { new ClientAuthorizationSettings { Id = "mock", AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.Basic, Username = "x", Password = "x" } },
            Originators = new List<Originator> { new Originator { Name = "mock-service", AuthenticationMethod = "mock", TokenUrl = "http://example.org/refresh-token" } }
        };


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
            _httpClientMock.SetupGet(x => x.SimulateOutgoingCalls).Returns(true).Verifiable();

            _leverConfigurationMock = new Mock<ILeverConfiguration>();
            _leverConfigurationMock.Setup(x => x.Value<int?>(nameof(AnonymousSchema.SchemaVersion))).Returns(1);
            _leverConfigurationMock.Setup(x => x.MandatoryValue<string>("ConnectionString")).Returns(RequestQueueHelper.MemoryQueueConnectionString);
            _leverConfigurationMock.Setup(x => x.Value<AuthenticationSettings>("Authentication")).Returns(AuthenticationSettings);

            _serviceAuthenticationHelperMock = new Mock<IServiceAuthenticationHelper>();
            _serviceAuthenticationHelperMock
                .Setup(x => x.GetAuthorizationForClientAsync(It.IsAny<Tenant>(), It.IsAny<ClientAuthorizationSettings>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AuthorizationToken
                {
                    Token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{AuthenticationSettings.Methods.First().Username}:{AuthenticationSettings.Methods.First().Password}")),
                    Type = "Basic"
                })
                .Verifiable();

            _queueAction = async rawEnvelope =>
            {
                var envelope = await RequestEnvelope.FromRawAsync(rawEnvelope, TimeSpan.FromSeconds(1));
                Log.LogInformation($"Action on {rawEnvelope.RawRequest.Title}");

                // Process Out and Callback
                var handler = await CreateRequestHandler(envelope, CancellationToken.None);
                await handler.ProcessOneRequestAsync();
            };
        }

        [TestCleanup]
        public void ClassCleanup()
        {
            _runBackgroundJob = false;
        }

        private async Task<RequestHandler> CreateRequestHandler(RequestEnvelope envelope, CancellationToken cancellationToken)
        {
            var rawEnvelope = await envelope.ToRawAsync(cancellationToken);
            return new RequestHandler(_httpClientMock.Object, Tenant, _leverConfigurationMock.Object, rawEnvelope, _serviceAuthenticationHelperMock?.Object);
        }

        [TestMethod]
        public async Task ResponseCode_From_Failed_Request_Is_ApplicationJson_In_Callback()
        {
            // Arrange
            var resetEvent = new ManualResetEvent(false);

            string callbackResponseString = null;
            const string expectedPayload = "Cannot work with that";
            const string expecteMediaType = "text/plain";

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
            Assert.IsTrue(resetEvent.WaitOne(TimeSpan.FromMilliseconds(1000)), "This asynchronous request didn't finish in time");

            Assert.IsNotNull(callbackResponseString, "We need the response content of the Callback request");

            var responseContent = JsonConvert.DeserializeObject<Nexus.Link.AsyncCaller.Sdk.Common.Models.ResponseContent>(callbackResponseString);
            Assert.IsNotNull(responseContent);
            Assert.AreEqual(HttpStatusCode.BadRequest, responseContent.StatusCode);
            Assert.AreEqual(expecteMediaType, responseContent.PayloadMediaType);
            Assert.AreEqual(expectedPayload, responseContent.Payload);
        }

        [TestMethod]
        public async Task Expired_Authentication_Can_Be_Refreshed()
        {
            // Arrange
            var oldToken = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Name, AuthenticationSettings.Originators.First().Name) }),
                NotBefore = DateTime.UtcNow.AddHours(-2),
                Expires = DateTime.UtcNow.AddHours(-1)
            });

            var request = new Request
            {
                CallOut = new HttpRequestMessage(HttpMethod.Post, "https://example.com/out"),
                Context = "sub-123"
            };
            request.CallOut.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oldToken);
            var requestEnvelope = new RequestEnvelope(Tenant.Organization, Tenant.Environment, request, TimeSpan.FromMinutes(1));

            var resetEvent = new ManualResetEvent(false);

            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback((HttpRequestMessage httpRequest, CancellationToken ct) =>
                {
                    Log.LogInformation($"Http client mock: {httpRequest.RequestUri}");

                    // When the request is done, make sure we have a valid token
                    if (httpRequest.RequestUri.ToString().Contains("/out"))
                    {
                        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(httpRequest.Headers.Authorization.Parameter);
                        Log.LogInformation($"JWT valid to: {jwt.ValidTo}");
                        if (jwt.ValidTo > DateTime.UtcNow) resetEvent.Set();
                    }
                })
                .ReturnsAsync((HttpRequestMessage httpRequest, CancellationToken ct) =>
                {
                    // When calling auth refresh url
                    if (httpRequest.RequestUri.ToString().Equals(AuthenticationSettings.Originators.First().TokenUrl))
                    {
                        var bearerToken = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Name, AuthenticationSettings.Originators.First().Name) }),
                            Expires = DateTime.UtcNow.AddHours(3)
                        });
                        var refreshResult = new RefreshAuthenticationResult
                        {
                            Headers = new List<RefreshAuthenticationHeader> { new RefreshAuthenticationHeader { Name = "Authorization", Value = new List<string> { $"Bearer {bearerToken}" } } }
                        };
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(refreshResult), Encoding.UTF8, "application/json")
                        };
                    }

                    // When calling CallOut
                    if (httpRequest.RequestUri.ToString().Contains("/out"))
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }

                    throw new FulcrumAssertionFailedException($"Unknown request url: {httpRequest.RequestUri}");
                });


            // Act
            await _queue.AddMessageAsync((await requestEnvelope.ToRawAsync()).Serialize());

            // Assert
            Assert.IsTrue(resetEvent.WaitOne(TimeSpan.FromMilliseconds(2000)), "This asynchronous request didn't finish in time");
            _serviceAuthenticationHelperMock.Verify();
        }

        [TestMethod]
        public async Task Gives_Up_If_Token_Is_Expired_And_No_IServiceAuthenticationHelper_Is_Present()
        {
            // Arrange
            _serviceAuthenticationHelperMock = null;

            var oldToken = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Name, AuthenticationSettings.Originators.First().Name) }),
                NotBefore = DateTime.UtcNow.AddHours(-2),
                Expires = DateTime.UtcNow.AddHours(-1)
            });

            var request = new Request
            {
                CallOut = new HttpRequestMessage(HttpMethod.Post, "https://example.com/out"),
                Context = "sub-123"
            };
            request.CallOut.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oldToken);
            var requestEnvelope = new RequestEnvelope(Tenant.Organization, Tenant.Environment, request, TimeSpan.FromMinutes(1));

            // Act
            await _queue.AddMessageAsync((await requestEnvelope.ToRawAsync()).Serialize());

            // Assert
            await Task.Delay(500);
            _httpClientMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task Gives_Up_If_Token_Is_Expired_And_No_AuthenticationSettings_Are_Present()
        {
            // Arrange
            _leverConfigurationMock.Setup(x => x.Value<AuthenticationSettings>("Authentication")).Returns((AuthenticationSettings)null);

            var oldToken = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Name, AuthenticationSettings.Originators.First().Name) }),
                NotBefore = DateTime.UtcNow.AddHours(-2),
                Expires = DateTime.UtcNow.AddHours(-1)
            });

            var request = new Request
            {
                CallOut = new HttpRequestMessage(HttpMethod.Post, "https://example.com/out"),
                Context = "sub-123"
            };
            request.CallOut.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oldToken);
            var requestEnvelope = new RequestEnvelope(Tenant.Organization, Tenant.Environment, request, TimeSpan.FromMinutes(1));

            // Act
            await _queue.AddMessageAsync((await requestEnvelope.ToRawAsync()).Serialize());

            // Assert
            await Task.Delay(500);
            _httpClientMock.VerifyNoOtherCalls();
        }
    }
}
