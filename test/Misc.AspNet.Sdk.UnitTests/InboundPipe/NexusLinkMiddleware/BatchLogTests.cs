#if NETCOREAPP
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Misc.AspNet.Sdk.Inbound;

#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.NexusLinkMiddleware
{
    [TestClass]
    public class BatchLogTests
    {
        private static int _logCounter;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(BatchLogTests).FullName);
            FulcrumApplication.Context.CorrelationId = null;
            FulcrumApplication.Context.ClientTenant = null;
            FulcrumApplication.Context.LeverConfiguration = null;
        }

        [TestMethod]
        public async Task BatchLogs()
        {
            _logCounter = 0;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger.Setup(logger =>
                    logger.LogSync(
                        It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    Assert.IsTrue(FulcrumApplication.Context.IsInBatchLogger);
                    Interlocked.Increment(ref _logCounter);
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = new BatchLogger(mockLogger.Object);
            FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Information;

            var doLogging = new LogFiveTimesHandler(async c => await Task.CompletedTask);
            var options = new NexusLinkMiddlewareOptions();
            options.Features.BatchLog.Enabled = true;
            options.Features.BatchLog.Threshold = LogSeverityLevel.Warning;
            var handler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(doLogging.InvokeAsync, options);
            var context = new DefaultHttpContext();
            Assert.IsFalse(FulcrumApplication.Context.IsInBatchLogger);
            await handler.InvokeAsync(context);
            Assert.IsFalse(FulcrumApplication.Context.IsInBatchLogger);
            mockLogger.Verify();
        }

        private class LogFiveTimesHandler
        {
            private readonly RequestDelegate _next;

            public LogFiveTimesHandler(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                Log.LogVerbose("Verbose");
                Assert.AreEqual(0, _logCounter);
                Log.LogInformation("Information");
                Assert.AreEqual(0, _logCounter);
                Log.LogWarning("Warning");
                Assert.AreEqual(3, _logCounter);
                Log.LogError("Error");
                Assert.AreEqual(4, _logCounter);
                Log.LogCritical("Critical");
                Assert.AreEqual(5, _logCounter);
                await _next(context);
            }
        }
    }
}
#endif