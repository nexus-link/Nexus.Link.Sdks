using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc.Web.Sdk.Outbound;
using Misc.Web.Sdk.Outbound.Options;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Pipe;

namespace Misc.Web.Sdk.UnitTests.Outbound
{
    [TestClass]
    public class BusinessApiOutPipeTests
    {
        private NexusLinkHandlerOptions _options;
        private NexusLinkHandler _handler;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(BusinessApiOutPipeTests).FullName);
            _options = new NexusLinkHandlerOptions
            {
                Features = new HandlerFeatures
                {
                    ForwardNexusUserAuthorization = { Enabled = true },
                    ForwardNexusTranslatedUserId = { Enabled = true }
                }
            };
            _handler = new NexusLinkHandler(_options);
        }

        /// <summary>
        /// Given that "NexusTranslatedUserId" is setup on context, we expect it propagated as a header
        /// </summary>
        [TestMethod]
        public async Task Translated_UserId_Is_Propagated()
        {
            // Arrange
            HttpRequestHeaders actualHeaders = null;
            const string translatedUserId = "t-123";
            FulcrumApplication.Context.ValueProvider.SetValue(Constants.TranslatedUserIdKey, translatedUserId);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");
            await _handler.TestSendAsync(request, (req, cancellationToken) =>
            {
                actualHeaders = req.Headers;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            // Assert
            Assert.IsNotNull(actualHeaders);
            Assert.IsTrue(actualHeaders.TryGetValues(Constants.NexusTranslatedUserIdHeaderName, out var header));
            Assert.AreEqual(translatedUserId, header.First());
        }

        /// <summary>
        /// Given that "NexusUserAuthorization" is setup on context, we expect it propagated as a header
        /// </summary>
        [TestMethod]
        public async Task User_Authorization_Is_Propagated()
        {
            // Arrange
            HttpRequestHeaders actualHeaders = null;
            const string userAuthorization = "eymannen";
            FulcrumApplication.Context.ValueProvider.SetValue(Constants.NexusUserAuthorizationKeyName, userAuthorization);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");;
            await _handler.TestSendAsync(request, (req, cancellationToken) =>
            {
                actualHeaders = req.Headers;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            // Assert
            Assert.IsNotNull(actualHeaders);
            Assert.IsTrue(actualHeaders.TryGetValues(Constants.NexusUserAuthorizationHeaderName, out var header));
            Assert.AreEqual(userAuthorization, header.First());
        }
    }
}
