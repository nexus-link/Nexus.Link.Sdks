using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Misc.Web.Sdk;
using Shouldly;

namespace Misc.Web.Sdk.UnitTests
{
    [TestClass]
    public class NexusHttpSenderTest
    {
        private Mock<IHttpClient> _httpClientMock;
        private HttpRequestMessage _actualRequestMessage;
        private string _actualContent;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(NexusHttpSenderTest).FullName);
        }

        [TestMethod]
        public void EmptyBaseUriHasCorrectUrl()
        {
            const string baseUri = "";
            var _ = new NexusHttpSender(baseUri);
        }

        [TestMethod]
        public void EmptyBaseUri()
        {
            const string baseUri = "";
            var _ = new NexusHttpSender(baseUri);
        }

        [TestMethod]
        public void NullBaseUri()
        {
            const string baseUri = null;
            var _ = new NexusHttpSender(baseUri);
        }

        [TestMethod]
        public void WhitespaceBaseUri()
        {
            const string baseUri = "  ";
            var _ = new NexusHttpSender(baseUri);
        }

        [DataTestMethod]
        [DataRow("", "https://example.com", "https://example.com")]
        [DataRow(" ", "https://example.com", "https://example.com")]
        [DataRow(null, "https://example.com", "https://example.com")]
        [DataRow("https://example.com", "", "https://example.com")]
        [DataRow("https://example.com", " ", "https://example.com")]
        [DataRow("https://example.com", "/tests", "https://example.com/tests")]
        [DataRow("https://example.com", "tests", "https://example.com/tests")]
        [DataRow("https://example.com", "?test=123", "https://example.com?test=123")]
        [DataRow("https://example.com", null, "https://example.com")]
        public async Task BaseUrlAndRelativeUrlTests(string baseUri, string relativeUrl, string expectedUrl)
        {
            // Arrange
            var sender = new NexusHttpSender(baseUri);
            sender.NexusLinkHandlerOptions.Features.CustomSendDelegate.Enabled = true;
            sender.NexusLinkHandlerOptions.Features.CustomSendDelegate.SendAsyncDelegate = (r, ct) =>
            {
                _actualRequestMessage = r;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }; 

            // Act
            await sender.SendRequestAsync(HttpMethod.Get, relativeUrl);

            // Assert
            Assert.AreEqual(expectedUrl, _actualRequestMessage.RequestUri.OriginalString);
        }

        [DataTestMethod]
        [DataRow("", "tests/123")]
        [DataRow(" ", "tests/123")]
        [DataRow(null, "tests/123")]
        public async Task BaseUrlAndRelativeUrlTests_Throws(string baseUrl, string relativeUrl)
        {
            var sender = new NexusHttpSender(baseUrl);
            await Assert.ThrowsExceptionAsync<FulcrumContractException>(() =>
                sender.SendRequestAsync(HttpMethod.Get, "relativeUrl"));
        }

        [TestMethod]
        public void RelativePath()
        {
            const string baseUri = "http://example.se";
            var baseHttpSender = new NexusHttpSender(baseUri);
            Assert.AreEqual($"{baseUri}/", baseHttpSender.BaseUri?.AbsoluteUri);
            const string relativeUrl = "Test";
            var relativeHttpSender = baseHttpSender.CreateHttpSender(relativeUrl);
            Assert.AreEqual($"{baseUri}/{relativeUrl}", relativeHttpSender.BaseUri?.AbsoluteUri);
        }

        [TestMethod]
        public void QuestionMark()
        {
            const string baseUri = "http://example.se";
            var baseHttpSender = new NexusHttpSender(baseUri);
            Assert.AreEqual($"{baseUri}/", baseHttpSender.BaseUri?.AbsoluteUri);
            const string relativeUrl = "?a=Test";
            var relativeHttpSender = baseHttpSender.CreateHttpSender(relativeUrl);
            Assert.AreEqual($"{baseUri}/{relativeUrl}", relativeHttpSender.BaseUri?.AbsoluteUri);
        }

        [TestMethod]

        public void BaseEndsInSlash()
        {
            const string baseUri = "http://example.se/";
            var baseHttpSender = new NexusHttpSender(baseUri);
            Assert.AreEqual(baseUri, baseHttpSender.BaseUri?.AbsoluteUri);
            const string relativeUrl = "Test";
            var relativeHttpSender = baseHttpSender.CreateHttpSender(relativeUrl);
            Assert.AreEqual($"{baseUri}{relativeUrl}", relativeHttpSender.BaseUri?.AbsoluteUri);
        }

        [TestMethod]
        public async Task JTokenAsBody()
        {
            // Arrange
            const string baseUri = "http://example.se/";
            var contentAsObject = new TestType { A = "The string", B = 113 };
            var contentAsJson = JsonConvert.SerializeObject(contentAsObject);
            var contentAsJToken = JToken.Parse(contentAsJson);
            var sender = new NexusHttpSender(baseUri);
            sender.NexusLinkHandlerOptions.Features.CustomSendDelegate.Enabled = true;
            sender.NexusLinkHandlerOptions.Features.CustomSendDelegate.SendAsyncDelegate = async (r, ct) =>
            {
                _actualContent = await r.Content.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.OK);
            };

            // Act
            var response = await sender.SendRequestAsync(HttpMethod.Post, "", contentAsJToken);
            var actualContentAsObject = JsonConvert.DeserializeObject<TestType>(_actualContent);

            // Assert
            Assert.AreEqual(contentAsObject.A, actualContentAsObject.A);
            Assert.AreEqual(contentAsObject.B, actualContentAsObject.B);
        }

        [TestMethod]
        public async Task PostResponseWithNoContent()
        {
            // Arrange
            const string baseUri = "http://example.se/";
            var content = "content";
            var sender = new NexusHttpSender(baseUri);
            sender.NexusLinkHandlerOptions.Features.CustomSendDelegate.Enabled = true;
            sender.NexusLinkHandlerOptions.Features.CustomSendDelegate.SendAsyncDelegate = async (r, ct) =>
            {
                _actualContent = await r.Content.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            };

            // Act
            var response = await sender.SendRequestAsync<string, string>(HttpMethod.Post, "", content);

            // Assert
            Assert.IsNull(response.Body);
        }

        [DataRow(":method")]
        [DataRow("Content-Type")]
        [DataTestMethod]
        public async Task BadHeaderNames(string headerName)
        {
            const string baseUri = "http://example.se";
            var baseHttpSender = new NexusHttpSender(baseUri);
            var headers = new Dictionary<string, List<string>> { { headerName, new List<string> { "value" } } };
            var request = await baseHttpSender.CreateRequestAsync(HttpMethod.Post, "relative", headers);

        }

        [TestMethod]
        public async Task CanAccessCredentialsAtSendAsync()
        {
            // Arrange
            const string baseUri = "http://example.se";
            var credentials = new BasicAuthenticationCredentials { UserName = "foo" };
            var sender = new NexusHttpSender(baseUri, credentials);
            sender.NexusLinkHandlerOptions.Features.CustomSendDelegate.Enabled = true;
            sender.NexusLinkHandlerOptions.Features.CustomSendDelegate.SendAsyncDelegate = (r, ct) =>
            {
                _actualRequestMessage = r;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };
            var request = new HttpRequestMessage(HttpMethod.Get, baseUri);

            // Act
            await sender.Credentials.ProcessHttpRequestAsync(request, default);
            await sender.SendAsync(request);

            // Assert
            _actualRequestMessage.Headers.Authorization.ShouldNotBeNull();
        }
    }

    public class TestType
    {
        public string A { get; set; }
        public int B { get; set; }
    }
}