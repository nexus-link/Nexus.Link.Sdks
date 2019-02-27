using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Xlent.Lever.KeyTranslator.Sdk.Test
{
    [TestClass]
    public class TranslateClientTest
    {
        private TranslateClient _client;

        private static readonly Tenant Tenant = new Tenant("org", "env");
        private Mock<IHttpClient> _httpClientMock;
        private HttpRequestMessage _request;

        private const string SourceInstancePath = "(person.id!internet!karl+anka@kula.se)";

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(TranslateClientTest));

            _client = new TranslateClient("http://example.com", Tenant, new BasicAuthenticationCredentials());
            _httpClientMock = new Mock<IHttpClient>();
            RestClient.HttpClient = _httpClientMock.Object;


            _httpClientMock
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback((HttpRequestMessage req, CancellationToken token) => { _request = req; })
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(""),
                    RequestMessage = _request
                });
        }

        [TestMethod]
        public async Task UrlEncodingInTranslateToContextOrLock()
        {
            await _client.TranslateToContextOrLockAsync(SourceInstancePath, "crm");

            Assert.IsNotNull(_request);
            var encoded = WebUtility.UrlEncode(SourceInstancePath) ?? "";
            Assert.IsTrue(_request.RequestUri.ToString().Contains(encoded), _request.RequestUri.ToString());
        }

        [TestMethod]
        public async Task UrlEncodingInTranslateToClientOrLockAsync()
        {
            await _client.TranslateToClientOrLockAsync(SourceInstancePath, "crm");

            Assert.IsNotNull(_request);
            var encoded = WebUtility.UrlEncode(SourceInstancePath) ?? "";
            Assert.IsTrue(_request.RequestUri.ToString().Contains(encoded), _request.RequestUri.ToString());
        }

        [TestMethod]
        public async Task ReleaseLockAsync()
        {
            await _client.ReleaseLockAsync(SourceInstancePath, "crm");

            Assert.IsNotNull(_request);
            var encoded = WebUtility.UrlEncode(SourceInstancePath) ?? "";
            Assert.IsTrue(_request.RequestUri.ToString().Contains(encoded), _request.RequestUri.ToString());
        }
    }
}