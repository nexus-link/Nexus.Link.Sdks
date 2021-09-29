using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Moq;
using Moq.Language.Flow;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.AsyncManager.Sdk.RestClients;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Xunit;

namespace AsyncManager.Sdk.UnitTests
{
    public class AsyncRequestClientTests
    {
        private Mock<IHttpSender> _httpSenderMock;
        private readonly Tenant _tenant = new Tenant("test", "test");
        private readonly IAsyncRequestMgmtCapability _restClients;

        public AsyncRequestClientTests()
        {
            _httpSenderMock = new Mock<IHttpSender>();
            FulcrumApplicationHelper.UnitTestSetup(nameof(AsyncRequestClientTests));
            // TODO: Split tests for AsyncRequestMgmtRestClients and AsyncRequestClient
            _restClients = new AsyncRequestMgmtRestClients(_httpSenderMock.Object);
        }

        [Fact]
        public void CreateClient()
        {
            // act
            var client = new AsyncRequestClient(_restClients);

            // assert
            Assert.NotNull(client);
        }


        [Fact]
        public async Task SendRequestCallsHttpSender()
        {
            // arrange
            SetupSenderMockResponse(Guid.NewGuid().ToString())
                .Verifiable();
            var client = new AsyncRequestClient(_restClients);
            var request = TestDataGenerator.CreateDefaultAsyncHttpRequest(client);

            // act
            var actualRequestId = await client.SendRequestAsync(request);

            // assert
            _httpSenderMock.Verify();
        }

        [Fact]
        public async Task SendRequestReturnsExpectedRequestId()
        {
            // arrange
            var expectedRequestId = Guid.NewGuid().ToString();
            var actualRequest = TestDataGenerator.DefaultAsyncHttpRequest;
            var client = new AsyncRequestClient(_restClients);
            var request = TestDataGenerator.CreateDefaultAsyncHttpRequest(client);
            SetupSenderMockResponse(expectedRequestId)
                .Verifiable();

            // act
            var actualRequestId = await client.SendRequestAsync(request);

            // assert
            Assert.Equal(expectedRequestId, actualRequestId);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.CorrectRequests), MemberType = typeof(TestDataGenerator))]
        public async Task CorrectHttpRequest(AsyncHttpRequest expectedRequest)
        {
            // arrange
            var expectedRequestId = Guid.NewGuid().ToString();
            var client = new AsyncRequestClient(_restClients);
            SetupSenderMockResponse(expectedRequestId)
                .Callback((HttpMethod method, string url, HttpRequestCreate body, Dictionary<string, List<string>> headers,
                    CancellationToken token) =>
                {
                    AssertHelper.AssertEqual(expectedRequest, body);
                })
                .Verifiable();

            // act & assert
            var actualRequestId = await client.SendRequestAsync(expectedRequest);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.IncorrectRequests), MemberType = typeof(TestDataGenerator))]
        public async Task IncorrectHttpRequest(AsyncHttpRequest incorrectRequest)
        {
            // arrange
            var expectedRequestId = Guid.NewGuid();
            var client = new AsyncRequestClient(_restClients);

            // act & assert
            await Assert.ThrowsAsync<FulcrumContractException>(() => client.SendRequestAsync(incorrectRequest));
        }

        private IReturnsResult<IHttpSender> SetupSenderMockResponse(string expectedRequestId)
        {
            return _httpSenderMock.Setup(sender => sender.SendRequestAsync<string, HttpRequestCreate>(
                    It.IsAny<HttpMethod>(),
                    It.IsAny<string>(),
                    It.IsAny<HttpRequestCreate>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    var stringContent = new StringContent(expectedRequestId, Encoding.UTF8);
                    var response = new HttpOperationResponse<string>
                    {
                        Body = expectedRequestId,
                        Response = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = stringContent
                        }
                    };
                    return response;
                });
        }
    }
}
