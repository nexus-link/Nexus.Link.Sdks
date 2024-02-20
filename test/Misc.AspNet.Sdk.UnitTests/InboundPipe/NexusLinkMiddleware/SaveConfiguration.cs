#if NETCOREAPP
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Misc.AspNet.Sdk.Inbound;
using Nexus.Link.Misc.AspNet.Sdk.Inbound.Options;

#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.NexusLinkMiddleware
{
    [TestClass]
    public class SaveConfiguration
    {
        private static string _foundCorrelationId;

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(SaveConfiguration).FullName);
            FulcrumApplication.Context.CorrelationId = null;
            FulcrumApplication.Context.ClientTenant = null;
            FulcrumApplication.Context.LeverConfiguration = null;
        }

        /// <summary>
        /// Make sure <see cref="SaveConfiguration"/> propagates correlation id in case of any logging. 
        /// </summary>
        [TestMethod]
        public async Task SavedConfigurationHandlesNoCorrelationId()
        {
            // Setup a mocked logger as the FullLogger so that we have full control
            var mockLogger = new Mock<ISyncLogger>();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            mockLogger.Setup(x => x.LogSync(It.IsAny<LogRecord>())).Verifiable();

            // The expected correlation id propagated as a request header
            const string corrId = "CorrelationId";

            var leverConfig = new Mock<ILeverServiceConfiguration>();
            var options = new NexusLinkMiddlewareOptions();
            options.Features.SaveClientTenant.Enabled = true;
            options.Features.SaveClientTenant.RegexForFindingTenantInUrl = SaveClientTenantOptions.LegacyVersion;
            options.Features.SaveTenantConfiguration.Enabled = true;
            options.Features.SaveTenantConfiguration.ServiceConfiguration = leverConfig.Object;
            options.Features.SaveCorrelationId.Enabled = true;

            var handler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(ctx =>
            {
                _foundCorrelationId = FulcrumApplication.Context.CorrelationId;
                return Task.CompletedTask;
            }, options);
            var url = "https://v-mock.org/v2/smoke-testing-company/ver/";

            // Simulate an incoming request
            var context = new DefaultHttpContext();
            context.SetRequest(url);
            context.Request.Headers.Add("X-Correlation-ID", corrId);
            await handler.InvokeAsync(context);

            // Check that LogAsync has NOT been invoked
            mockLogger.VerifyNoOtherCalls();

            Assert.AreEqual(corrId, _foundCorrelationId,
                "When SaveConfiguration is run before SaveCorrelationId, we still expect X-Correlation-ID header to be handled");
        }
    }
}
#endif