using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Misc.Web.Sdk.Outbound;
using Nexus.Link.Misc.Web.Sdk.Outbound.Options;
using Shouldly;

#pragma warning disable CS0618

namespace Misc.Web.Sdk.UnitTests.Outbound
{
    [TestClass]
    public class HandleExecutionInformationTests
    {
        private NexusLinkHandlerOptions _options;
        private NexusLinkHandler _handler;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(HandleExecutionInformationTests).FullName);
            _options = new NexusLinkHandlerOptions();
            _handler = new NexusLinkHandler(_options);
        }
        
        [TestMethod]
        public async Task SaveBeforeAfter_Given_Enabled_Gives_Called()
        {
            //
            // Arrange
            //
            HandleExecutionInformationOptions.BeforeExecution actualBefore = null;
            HandleExecutionInformationOptions.AfterExecution actualAfter = null;

            // Create a handler with options
            _options.Features.HandleExecutionInformation.Enabled = true;
            _options.Features.HandleExecutionInformation.SaveBeforeExecutionAsyncDelegate =
                (before, _) => Task.FromResult(actualBefore = before);
            _options.Features.HandleExecutionInformation.SaveAfterExecutionAsyncDelegate =
                (after, _) => Task.FromResult(actualAfter = after);

            // Set context
            var expectedParentExecutionId = Guid.NewGuid().ToString();
            FulcrumApplication.Context.ExecutionId = expectedParentExecutionId;
            var expectedExecutionId = Guid.NewGuid().ToString();
            FulcrumApplication.Context.ChildExecutionId = expectedExecutionId;
            var expectedRequestDescription = Guid.NewGuid().ToString();
            FulcrumApplication.Context.ChildRequestDescription = expectedRequestDescription;


            // Simulate a http request
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/request");

            // Act
            await _handler.TestSendAsync(request, (req, cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            //
            // Assert
            //
            actualBefore.ShouldNotBeNull();
            actualBefore.ExecutionId.ShouldBe(expectedExecutionId);
            actualBefore.ParentExecutionId.ShouldBe(expectedParentExecutionId);
            actualBefore.RequestDescription.ShouldNotBeNull();
            actualBefore.RequestDescription.ShouldBe(expectedRequestDescription);

            actualAfter.ShouldNotBeNull();
            actualAfter.ExecutionId.ShouldBe(expectedExecutionId);
            actualAfter.ResponseDescription.ShouldNotBeNull();
        }
    }
}
