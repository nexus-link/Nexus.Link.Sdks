#if NETCOREAPP
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Misc.AspNet.Sdk.Inbound;
using Shouldly;
#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.NexusLinkMiddleware
{
    [TestClass]
    public class SaveReentryAuthentication
    {
        private string _foundReentryAuthentication;

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(SaveReentryAuthentication).FullName);
            FulcrumApplication.Context.ReentryAuthentication = null;
        }

        [TestMethod]
        public async Task SaveReentryAuthentication_Given_NoHeader_Gives_Null()
        {
            // Arrange
            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveReentryAuthentication.Enabled = true;
            var context = new DefaultHttpContext();
            context.SetRequestWithReentryAuthentication(null);
            var handler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(ctx =>
            {
                _foundReentryAuthentication = FulcrumApplication.Context.ReentryAuthentication;
                return Task.CompletedTask;
            }, options);

            // Act
            await handler.InvokeAsync(context);

            // Assert
            _foundReentryAuthentication.ShouldBeNull();
        }

        [TestMethod]
        public async Task SaveReentryAuthentication_Given_Header_Gives_ContextSet()
        {
            // Arrange
            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveReentryAuthentication.Enabled = true;
            var context = new DefaultHttpContext();
            var expectedKey = Guid.NewGuid().ToGuidString();
            context.SetRequestWithReentryAuthentication(expectedKey);
            var handler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(ctx =>
            {
                _foundReentryAuthentication = FulcrumApplication.Context.ReentryAuthentication;
                return Task.CompletedTask;
            }, options);

            // Act
            await handler.InvokeAsync(context);

            // Assert
            _foundReentryAuthentication.ShouldNotBeNull();
            _foundReentryAuthentication.ShouldBe(expectedKey);
        }
    }
}
#endif