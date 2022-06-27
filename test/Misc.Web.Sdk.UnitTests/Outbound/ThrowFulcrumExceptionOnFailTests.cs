using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Misc.Web.Sdk.Outbound;
using Shouldly;

namespace Misc.Web.Sdk.UnitTests.Outbound
{
    [TestClass]
    public class ThrowFulcrumExceptionOnFailTests
    {
        private NexusLinkHandlerOptions _options;
        private NexusLinkHandler _handler;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ThrowFulcrumExceptionOnFailTests).FullName);
            _options = new NexusLinkHandlerOptions();
            _handler = new NexusLinkHandler(_options);
        }

        /// <summary>
        /// Given that "NexusTranslatedUserId" is setup on context, we expect it propagated as a header
        /// </summary>
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Given_OKResponse_Gives_NoException(bool enabled)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");
            var statusCode = HttpStatusCode.OK;
            var expectedContent = Guid.NewGuid().ToGuidString();
            var responseMessage = new HttpResponseMessage(statusCode);
            responseMessage.Content = new StringContent(expectedContent);
            _options.Features.ThrowFulcrumExceptionOnFail.Enabled = enabled;

            // Act
            var response = await _handler.TestSendAsync(request, (req, cancellationToken) => Task.FromResult(responseMessage));

            // Assert
            var actualContent = await response.Content.ReadAsStringAsync();
            if (enabled)
            {
                actualContent.ShouldBe(expectedContent);
            }
            else
            {
                actualContent.ShouldBe(expectedContent);
            }
        }

        /// <summary>
        /// Given that "NexusTranslatedUserId" is setup on context, we expect it propagated as a header
        /// </summary>
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Given_InternalServerError_Gives_ExceptionIfEnabled(bool enabled)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");
            var statusCode = HttpStatusCode.InternalServerError;
            var expectedContent = Guid.NewGuid().ToGuidString();
            var responseMessage = new HttpResponseMessage(statusCode);
            responseMessage.Content = new StringContent(expectedContent);
            _options.Features.ThrowFulcrumExceptionOnFail.Enabled = enabled;

            // Act
            Exception actualException = null;
            try
            {
                await _handler.TestSendAsync(request,
                    (req, cancellationToken) => Task.FromResult(responseMessage));
            }
            catch (Exception e)
            {
                actualException = e;
            }

            // Assert
            if (enabled)
            {
                actualException.ShouldNotBeNull();
            }
            else
            {
                actualException.ShouldBeNull();
            }
        }

        /// <summary>
        /// Given that "NexusTranslatedUserId" is setup on context, we expect it propagated as a header
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public async Task Given_NotOkStatusCode_Gives_ExpectedException(HttpStatusCode statusCode, string content, Type expectedExceptionType)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");
            var responseMessage = new HttpResponseMessage(statusCode);
            responseMessage.Content = new StringContent(content);
            _options.Features.ThrowFulcrumExceptionOnFail.Enabled = true;

            // Act
            Exception actualException = null;
            try
            {
                await _handler.TestSendAsync(request,
                    (req, cancellationToken) => Task.FromResult(responseMessage));
            }
            catch (Exception e)
            {
                actualException = e;
            }

            // Assert
            actualException.ShouldNotBeNull();
            actualException.ShouldBeAssignableTo(expectedExceptionType);
        }

        public static IEnumerable<object[]> GetData()
        {
            yield return new object[] {
                HttpStatusCode.Accepted,
                JsonConvert.SerializeObject(new RequestAcceptedContent
                {
                    PollingUrl = Guid.NewGuid().ToGuidString(),
                    RegisterCallbackUrl = Guid.NewGuid().ToGuidString(),
                    RequestId = Guid.NewGuid().ToGuidString()
                }),
                typeof(RequestAcceptedException)
            };
            yield return new object[] {
                HttpStatusCode.Accepted,
                JsonConvert.SerializeObject(new RequestPostponedContent
                {
                    ReentryAuthentication = Guid.NewGuid().ToGuidString(),
                    WaitingForRequestIds = new List<string>(),
                    TryAgain = false,
                    TryAgainAfterMinimumSeconds = 60
                }),
                typeof(RequestPostponedException)
            };
            yield return new object[] {
                HttpStatusCode.Accepted,
                JsonConvert.SerializeObject("unknown content"),
#pragma warning disable CS0618
                typeof(FulcrumAcceptedException)
#pragma warning restore CS0618
            };
            yield return new object[] {
                HttpStatusCode.InternalServerError,
                JsonConvert.SerializeObject("unknown content"),
#pragma warning disable CS0618
                typeof(FulcrumResourceException)
#pragma warning restore CS0618
            };
            yield return new object[] {
                HttpStatusCode.InternalServerError,
                JsonConvert.SerializeObject(new FulcrumError
                {
                    Type = FulcrumAssertionFailedException.ExceptionType
                }),
#pragma warning disable CS0618
                typeof(FulcrumResourceException)
#pragma warning restore CS0618
            };
        }
    }
}
