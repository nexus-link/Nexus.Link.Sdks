using System;
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
        private ITranslateClient _client;

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
            HttpSender.HttpClient = _httpClientMock.Object;


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
#pragma warning disable CS0618 // Type or member is obsolete
            await _client.TranslateToContextOrLockAsync(SourceInstancePath, "crm");
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.IsNotNull(_request);
            var encoded = WebUtility.UrlEncode(SourceInstancePath) ?? "";
            Assert.IsTrue(_request.RequestUri.ToString().Contains(encoded), _request.RequestUri.ToString());
        }

        [TestMethod]
        public async Task UrlEncodingInTranslateToContextOrLock2()
        {
            await _client.TranslateToContextOrLock2Async(SourceInstancePath, "crm");

            Assert.IsNotNull(_request);
            var uri = _request.RequestUri.ToString();
            Assert.IsTrue(uri.Contains("?"), uri);
            var query = uri.Substring(uri.IndexOf("?", StringComparison.Ordinal));

            var encoded = WebUtility.UrlEncode(SourceInstancePath) ?? "";
            Assert.IsTrue(query.Contains(encoded), query);
        }

        [TestMethod]
        public async Task UrlEncodingInTranslateToClientOrLockAsync()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            await _client.TranslateToClientOrLockAsync(SourceInstancePath, "crm");
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.IsNotNull(_request);
            var encoded = WebUtility.UrlEncode(SourceInstancePath) ?? "";
            Assert.IsTrue(_request.RequestUri.ToString().Contains(encoded), _request.RequestUri.ToString());
        }

        [TestMethod]
        public async Task UrlEncodingInTranslateToClientOrLockAsync2()
        {
            await _client.TranslateToClientOrLock2Async(SourceInstancePath, "crm");

            Assert.IsNotNull(_request);
            var uri = _request.RequestUri.ToString();
            Assert.IsTrue(uri.Contains("?"), uri);
            var query = uri.Substring(uri.IndexOf("?", StringComparison.Ordinal));

            var encoded = WebUtility.UrlEncode(SourceInstancePath) ?? "";
            Assert.IsTrue(query.Contains(encoded), query);
        }

        [TestMethod]
        public async Task ReleaseLockAsync()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            await _client.ReleaseLockAsync(SourceInstancePath, "crm");
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.IsNotNull(_request);
            var encoded = WebUtility.UrlEncode(SourceInstancePath) ?? "";
            Assert.IsTrue(_request.RequestUri.ToString().Contains(encoded), _request.RequestUri.ToString());
        }

        [TestMethod]
        public async Task ReleaseLockAsync2()
        {
            await _client.ReleaseLock2Async(SourceInstancePath, "crm");

            Assert.IsNotNull(_request);
            Assert.IsFalse(_request.RequestUri.ToString().Contains("person"), _request.RequestUri.ToString());
        }
    }
}