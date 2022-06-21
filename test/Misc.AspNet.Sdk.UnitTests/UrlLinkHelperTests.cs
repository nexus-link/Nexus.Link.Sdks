using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Controllers;

namespace Misc.AspNet.Sdk.UnitTests
{
    [TestClass]
    public class UrlLinkHelperTests
    {
        public const string RouteName = "GetOneBanana";
        public const string Route = "api/bananas/{id}";

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(UrlLinkHelperTests).FullName);
        }

        #region Helpers

        private static UrlHelper CreateUrlHelper(string baseUrl)
        {
            var uri = new Uri(baseUrl);
            var httpContext = new DefaultHttpContext()
            {
                Request =
                {
                    Host = new HostString(uri.Host, uri.Port),
                    Path = new PathString(uri.PathAndQuery),
                    Scheme = uri.Scheme
                }
            };
            var routeData = new RouteData { Routers = { new MockRouter() } };
            var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
            return new UrlHelper(actionContext);
        }

        #endregion

        [TestMethod]
        public void ModifyToHttps()
        {
            var urlHelper = CreateUrlHelper("http://example.com");
            var uri = new Uri(urlHelper.LinkWithEnforcedHttps(RouteName, new { id = "33" }));

            Assert.IsTrue(uri.Scheme == Uri.UriSchemeHttps);
            Assert.IsFalse(uri.IsLoopback);
            Assert.IsTrue(uri.PathAndQuery.EndsWith("/33"));
        }

        [TestMethod]
        public void RemovePort()
        {
            var urlHelper = CreateUrlHelper("http://example.com:80");
            var uri = new Uri(urlHelper.LinkWithEnforcedHttps(RouteName, new { id = "33" }));

            Assert.IsTrue(uri.Scheme == Uri.UriSchemeHttps);
            Assert.IsFalse(uri.IsLoopback);
            Assert.IsTrue(uri.PathAndQuery.EndsWith("/33"));
            Assert.AreEqual(443, uri.Port);
            Assert.IsFalse(uri.PathAndQuery.Contains(":80"));
        }

        [TestMethod]
        public void NoModificationOnLocalHost1()
        {
            var urlHelper = CreateUrlHelper("http://localhost");
            var uri = new Uri(urlHelper.LinkWithEnforcedHttps(RouteName, new { id = "99" }));

            Assert.IsFalse(uri.Scheme == Uri.UriSchemeHttps);
            Assert.IsTrue(uri.IsLoopback);
            Assert.IsTrue(uri.PathAndQuery.EndsWith("/99"));
        }

        [TestMethod]
        public void NoModificationOnLocalHost2()
        {
            var urlHelper = CreateUrlHelper("http://127.0.0.1");
            var uri = new Uri(urlHelper.LinkWithEnforcedHttps(RouteName, new { id = "99" }));

            Assert.IsFalse(uri.Scheme == Uri.UriSchemeHttps);
            Assert.IsTrue(uri.IsLoopback);
            Assert.IsTrue(uri.PathAndQuery.EndsWith("/99"));
        }

        [TestMethod]
        public void NoModificationOnNexusLocal()
        {
            var urlHelper = CreateUrlHelper("http://nexus.local");
            var uri = new Uri(urlHelper.LinkWithEnforcedHttps(RouteName, new { id = "99" }));

            Assert.IsFalse(uri.Scheme == Uri.UriSchemeHttps);
            Assert.IsTrue(uri.PathAndQuery.EndsWith("/99"));
        }

        [TestMethod]
        public void NoModificationWhenHttps()
        {
            var urlHelper = CreateUrlHelper("https://example.com");
            var uri = new Uri(urlHelper.LinkWithEnforcedHttps(RouteName, new { id = "66" }));

            Assert.IsTrue(uri.Scheme == Uri.UriSchemeHttps);
            Assert.IsFalse(uri.IsLoopback);
            Assert.IsTrue(uri.PathAndQuery.EndsWith("/66"));
        }

        [TestMethod]
        public void NoModificationWhenHttpsOnLocalhost1()
        {
            var urlHelper = CreateUrlHelper("https://localhost");
            var uri = new Uri(urlHelper.LinkWithEnforcedHttps(RouteName, new { id = "11" }));

            Assert.IsTrue(uri.Scheme == Uri.UriSchemeHttps);
            Assert.IsTrue(uri.IsLoopback);
            Assert.IsTrue(uri.PathAndQuery.EndsWith("/11"));
        }

        [TestMethod]
        public void NoModificationWhenHttpsOnLocalhost2()
        {
            var urlHelper = CreateUrlHelper("https://127.0.0.1");
            var uri = new Uri(urlHelper.LinkWithEnforcedHttps(RouteName, new { id = "11" }));

            Assert.IsTrue(uri.Scheme == Uri.UriSchemeHttps);
            Assert.IsTrue(uri.IsLoopback);
            Assert.IsTrue(uri.PathAndQuery.EndsWith("/11"));
        }
    }

    #region Mocks

    internal class MockRouter : IRouter
    {
        public Task RouteAsync(RouteContext context)
        {
            return Task.CompletedTask;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            Assert.AreEqual(UrlLinkHelperTests.RouteName, context.RouteName);
            // There is probably a better way to achieve this without implementing an IRouter...
            return new VirtualPathData(this, UrlLinkHelperTests.Route.Replace("{id}", (string)context.Values.First().Value));
        }
    }

    #endregion
}
