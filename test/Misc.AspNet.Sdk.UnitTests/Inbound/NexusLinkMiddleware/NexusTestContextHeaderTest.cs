using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Misc.AspNet.Sdk.Inbound;
using Shouldly;

namespace Misc.AspNet.Sdk.UnitTests.Inbound.NexusLinkMiddleware
{
    [TestClass]
    public class NexusTestContextHeaderTest
    {
        private static string _foundContext;
        private Nexus.Link.Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware _itemUnderTest;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(NexusTestContextHeaderTest).FullName);

            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveNexusTestContext.Enabled = true;
            _itemUnderTest = new Nexus.Link.Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(ctx =>
            {
                _foundContext = FulcrumApplication.Context.NexusTestContext;
                return Task.CompletedTask;
            }, options);
        }

        [TestMethod]
        public async Task Header_Is_Setup_On_Context()
        {
            const string headerValue = "v1; test-id: abc-123";
            const string url = "https://example.com";

            var context = new DefaultHttpContext();
            context.SetRequest(url);
            context.Request.Headers.Add(Constants.NexusTestContextHeaderName, headerValue);
            await _itemUnderTest.InvokeAsync(context);
            _foundContext.ShouldBe(headerValue);
        }

        [TestMethod]
        public async Task No_Header_Is_Setup_On_Context()
        {
            const string url = "https://example.com";
            var context = new DefaultHttpContext();
            context.SetRequest(url);
            await _itemUnderTest.InvokeAsync(context);
            _foundContext.ShouldBeNull();
        }

        [TestMethod]
        public async Task Json_Value_Can_Be_Used()
        {
            var headerValue = JsonConvert.SerializeObject(new { Id = "123" });
            const string url = "https://example.com";

            var context = new DefaultHttpContext();
            context.SetRequest(url);
            context.Request.Headers.Add(Constants.NexusTestContextHeaderName, headerValue);
            await _itemUnderTest.InvokeAsync(context);
            _foundContext.ShouldBe(headerValue);
        }
    }
}