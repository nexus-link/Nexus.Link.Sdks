using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Misc.Web.Sdk.Outbound;
using Shouldly;

namespace Misc.Web.Sdk.UnitTests.Outbound
{
    [TestClass]
    public class ForwardHeadersTests
    {
        private NexusLinkHandlerOptions _options;
        private NexusLinkHandler _handler;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ForwardHeadersTests).FullName);
            _options = new NexusLinkHandlerOptions();
            _handler = new NexusLinkHandler(_options);
        }

        /// <summary>
        /// Given that "NexusTranslatedUserId" is setup on context, we expect it propagated as a header
        /// </summary>
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ForwardCorrelationId_RespectsEnabled_ProducesExpectedResult(bool enabled)
        {
            // Arrange
            var headerValue = Guid.NewGuid().ToGuidString();
            var headerName = Constants.FulcrumCorrelationIdHeaderName;
            HttpRequestHeaders actualHeaders = null;
            _options.Features.ForwardCorrelationId.Enabled = enabled;
            FulcrumApplication.Context.CorrelationId = headerValue;
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");

            // Act
            await _handler.TestSendAsync(request, (req, cancellationToken) =>
            {
                actualHeaders = req.Headers;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            // Assert
            if (enabled)
            {
                actualHeaders.ShouldNotBeNull();
                actualHeaders.ShouldContain(p => p.Key == headerName);
                actualHeaders.TryGetValues(headerName, out var header)
                    .ShouldBeTrue();
                header.FirstOrDefault().ShouldBe(headerValue);
            }
            else
            {
                actualHeaders.ShouldNotBe(null);
                actualHeaders.ShouldNotContain(p => p.Key == headerName);
            }
        }

        /// <summary>
        /// Given that "NexusTestContext" is setup on context, we expect it propagated as a header
        /// </summary>
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ForwardNexusTestContext_RespectsEnabled_ProducesExpectedResult(bool enabled)
        {
            // Arrange
            var headerValue = Guid.NewGuid().ToGuidString();
            var headerName = Constants.NexusTestContextHeaderName;
            HttpRequestHeaders actualHeaders = null;
            _options.Features.ForwardNexusTestContext.Enabled = enabled;
            FulcrumApplication.Context.NexusTestContext = headerValue;
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");

            // Act
            await _handler.TestSendAsync(request, (req, cancellationToken) =>
            {
                actualHeaders = req.Headers;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            // Assert
            if (enabled)
            {
                actualHeaders.ShouldNotBeNull();
                actualHeaders.ShouldContain(p => p.Key == headerName);
                actualHeaders.TryGetValues(headerName, out var header)
                    .ShouldBeTrue();
                header.FirstOrDefault().ShouldBe(headerValue);
            }
            else
            {
                actualHeaders.ShouldNotBe(null);
                actualHeaders.ShouldNotContain(p => p.Key == headerName);
            }
        }

        /// <summary>
        /// Given that "ExecutionId" is setup on context, we expect it is propagated as the ParentExecutionId header
        /// </summary>
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task HandleExecutionId_ParentHeader_RespectsEnabled_ProducesExpectedResult(bool enabled)
        {
            // Arrange
            var headerValue = Guid.NewGuid().ToGuidString();
            var headerName = Constants.ParentExecutionIdHeaderName;
            HttpRequestHeaders actualHeaders = null;
            _options.Features.HandleExecutionInformation.Enabled = enabled;
            FulcrumApplication.Context.ExecutionId = headerValue;
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");

            // Act
            await _handler.TestSendAsync(request, (req, cancellationToken) =>
            {
                actualHeaders = req.Headers;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            // Assert
            if (enabled)
            {
                actualHeaders.ShouldNotBeNull();
                actualHeaders.ShouldContain(p => p.Key == headerName);
                actualHeaders.TryGetValues(headerName, out var header)
                    .ShouldBeTrue();
                header.FirstOrDefault().ShouldBe(headerValue);
            }
            else
            {
                actualHeaders.ShouldNotBe(null);
                actualHeaders.ShouldNotContain(p => p.Key == headerName);
            }
        }

        /// <summary>
        /// Given that "ChildExecutionId" is setup on context, we expect it is propagated as the ExecutionId header
        /// </summary>
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task HandleExecutionId_Header_RespectsEnabled_ProducesExpectedResult(bool enabled)
        {
            // Arrange
            var headerValue = Guid.NewGuid().ToGuidString();
            var headerName = Constants.ExecutionIdHeaderName;
            HttpRequestHeaders actualHeaders = null;
            _options.Features.HandleExecutionInformation.Enabled = enabled;
            FulcrumApplication.Context.ChildExecutionId = headerValue;
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");

            // Act
            await _handler.TestSendAsync(request, (req, cancellationToken) =>
            {
                actualHeaders = req.Headers;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            // Assert
            if (enabled)
            {
                actualHeaders.ShouldNotBeNull();
                actualHeaders.ShouldContain(p => p.Key == headerName);
                actualHeaders.TryGetValues(headerName, out var header)
                    .ShouldBeTrue();
                header.FirstOrDefault().ShouldBe(headerValue);
            }
            else
            {
                actualHeaders.ShouldNotBe(null);
                actualHeaders.ShouldNotContain(p => p.Key == headerName);
            }
        }

        /// <summary>
        /// Given that "NexusTranslatedUserId" is setup on context, we expect it propagated as a header
        /// </summary>
        [DataTestMethod]
        [DataRow(true, Constants.TranslatedUserIdKey, Constants.NexusTranslatedUserIdHeaderName, "D2CFE93C-AFDA-418E-87C2-4D6CC2CFDE5F")]
        [DataRow(false, Constants.TranslatedUserIdKey, Constants.NexusTranslatedUserIdHeaderName, "9F49D6FD-82E0-4A6B-B967-1E48EB664018")]
        [DataRow(true, Constants.NexusUserAuthorizationKeyName, Constants.NexusUserAuthorizationHeaderName, "50250962-EFC9-4D9E-A19F-E9DA61F3A30F")]
        [DataRow(false, Constants.NexusUserAuthorizationKeyName, Constants.NexusUserAuthorizationHeaderName, "0A43A3E1-FF63-4A02-8599-D85E5EB4B35A")]
        public async Task HeadersWithHeaderNameKey_RespectsEnabled_ProducesExpectedResult(bool enabled, string headerNameKey, string headerName, string headerValue)
        {
            // Arrange
            HttpRequestHeaders actualHeaders = null;
            switch (headerNameKey)
            {
                case Constants.TranslatedUserIdKey:
                    _options.Features.ForwardNexusTranslatedUserId.Enabled = enabled;
                    break;
                case Constants.NexusUserAuthorizationKeyName:
                    _options.Features.ForwardNexusUserAuthorization.Enabled = enabled;
                    break;
            }
            FulcrumApplication.Context.ValueProvider.SetValue(headerNameKey, headerValue);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");
            await _handler.TestSendAsync(request, (req, cancellationToken) =>
            {
                actualHeaders = req.Headers;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

            // Assert
            if (enabled)
            {
                actualHeaders.ShouldNotBeNull();
                actualHeaders.ShouldContain(p => p.Key == headerName);
                actualHeaders.TryGetValues(headerName, out var header)
                    .ShouldBeTrue();
                header.FirstOrDefault().ShouldBe(headerValue);
            }
            else
            {
                actualHeaders.ShouldNotBe(null);
                actualHeaders.ShouldNotContain(p => p.Key == headerName);
            }
        }
    }
}
