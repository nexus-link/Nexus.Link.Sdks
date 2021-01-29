using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Nexus.Link.AsyncCaller.Sdk.Common.Models;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;

namespace AsyncCaller.Sdk.UnitTests
{
    [TestClass]
    public class EnvelopeTests
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(EnvelopeTests));
            FulcrumApplication.Setup.SynchronousFastLogger = new ConsoleLogger();
        }

        [TestMethod]
        public async Task LatestResponseWithCommaInServerHeaderIsOk()
        {
            const string content = "HTTP/1.1 504 Gateway Time-out\r\n" +
                                   "Server: nginx/1.15.3.1, Crow\r\n" +
                                   "Date: Thu, 28 Jan 2021 13:34:50 GMT\r\n" +
                                   "Connection: keep-alive\r\n" +
                                   "Content-Type: text/html; charset=utf-8\r\n" +
                                   "Content-Length: 190\r\n" +
                                   "\r\n" +
                                   "<html>\r\n" +
                                   "<head><title>504 Gateway Time-out</title></head>\r\n" +
                                   "<body bgcolor=\"white\">\r\n" +
                                   "<center><h1>504 Gateway Time-out</h1></center>\r\n" +
                                   "<hr><center>nginx/1.15.3.1 Crow</center>\r\n" +
                                   "</body>\r\n" +
                                   "</html>";

            var serializer = new MessageContentHttpMessageSerializer(true);
            var response = await serializer.DeserializeToResponseAsync(Encoding.UTF8.GetBytes(content));
            Assert.AreEqual(HttpStatusCode.GatewayTimeout, response.StatusCode);
            Assert.IsNotNull(response.Headers.Date);
            Assert.AreEqual("Thu, 28 Jan 2021 13:34:50 GMT", ((DateTimeOffset) response.Headers.Date).ToString("R"));
            Assert.AreEqual(1, response.Headers.Connection.Count);
            Assert.AreEqual("keep-alive", response.Headers.Connection.First());
            Assert.AreEqual(190, response.Content.Headers.ContentLength);
        }

        [TestMethod]
        [DataRow("Server: nginx/1.15.3.1, Crow\r\n")]
        [DataRow("Expires: -1\r\n")]
        public async Task ContentCanContainEscapedKeywordsWhenCompensatingForDotCoreBugs(string compensateHeader)
        {
            var content = "HTTP/1.1 504 Gateway Time-out\r\n" +
                          compensateHeader +
                          "Date: Thu, 28 Jan 2021 13:34:50 GMT\r\n" +
                          "Connection: keep-alive\r\n" +
                          "Content-Type: applicatioin/json; charset=utf-8\r\n" +
                          "Content-Length: 190\r\n" +
                          "\r\n" +
                          "{\r\n" +
                          "Server: \"jada, jada\",\r\n" +
                          "\"Foo\": \"bar\",\r\n" +
                          "Expires: -1\r\n" +
                          "}";

            var serializer = new MessageContentHttpMessageSerializer(true);
            var response = await serializer.DeserializeToResponseAsync(Encoding.UTF8.GetBytes(content));
            Assert.AreEqual(HttpStatusCode.GatewayTimeout, response.StatusCode);
            Assert.IsNotNull(response.Headers.Date);
            Assert.AreEqual("Thu, 28 Jan 2021 13:34:50 GMT", ((DateTimeOffset) response.Headers.Date).ToString("R"));

            var resultContent = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(resultContent);
            Assert.AreEqual(json["Server"], "jada, jada");
            Assert.AreEqual(json["Expires"], -1);
        }
    }
}
