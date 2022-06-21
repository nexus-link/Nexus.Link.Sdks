using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Misc.AspNet.Sdk.Inbound;
using Nexus.Link.Misc.AspNet.Sdk.Inbound.Options;
using Shouldly;

#pragma warning disable CS0618

namespace Misc.AspNet.Sdk.UnitTests.Inbound.NexusLinkMiddleware
{
    [TestClass]
    public class SaveExecutionInformationTests
    {
        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(SaveExecutionInformationTests).FullName);
        }

        /// <summary>
        /// Make sure <see cref="SaveConfiguration"/> propagates correlation id in case of any logging. 
        /// </summary>
        [TestMethod]
        public async Task SaveBeforeAfter_Given_Enabled_Gives_Called()
        {
            //
            // Arrange
            //
            SaveExecutionInformationOptions.BeforeExecution actualBefore = null;
            SaveExecutionInformationOptions.AfterExecution actualAfter = null;

            // Create a middleware with options
            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveExecutionInformation.Enabled = true;
            options.Features.SaveExecutionInformation.SaveBeforeExecutionAsyncDelegate =
                (before, _) => Task.FromResult(actualBefore = before);
            options.Features.SaveExecutionInformation.SaveAfterExecutionAsyncDelegate =
                (after, _) => Task.FromResult(actualAfter = after);
            var middleware = new Nexus.Link.Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(ctx => Task.CompletedTask, options);

            // Simulate a http context
            var context = new DefaultHttpContext();
            context.SetRequest();
            var expectedExecutionId = Guid.NewGuid().ToString();
            context.Request.Headers.Add(Constants.ExecutionIdHeaderName, expectedExecutionId);
            var expectedParentExecutionId = Guid.NewGuid().ToString();
            context.Request.Headers.Add(Constants.ParentExecutionIdHeaderName, expectedParentExecutionId);

            //
            // Act
            //
            await middleware.InvokeAsync(context);

            //
            // Assert
            //
            actualBefore.ShouldNotBeNull();
            actualBefore.ExecutionId.ShouldBe(expectedExecutionId);
            actualBefore.ParentExecutionId.ShouldBe(expectedParentExecutionId);
            actualBefore.RequestDescription.ShouldNotBeNull();

            actualAfter.ShouldNotBeNull();
            actualAfter.ExecutionId.ShouldBe(expectedExecutionId);
            actualAfter.ResponseDescription.ShouldNotBeNull();
        }
    }
}
