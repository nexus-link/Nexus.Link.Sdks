#if NETCOREAPP
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Misc.AspNet.Sdk.Inbound;
using Nexus.Link.Libraries.Web.Error.Logic;
#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.NexusLinkMiddleware
{
    [TestClass]
    public class LogRequestResponseTests
    {
        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(LogRequestResponseTests).FullName);
            FulcrumApplication.Context.CorrelationId = null;
            FulcrumApplication.Context.ClientTenant = null;
            FulcrumApplication.Context.LeverConfiguration = null;
        }

        /// <summary>
        /// LogRequestAndResponse must not come before BatchLogs in the pipe
        /// </summary>
        [TestMethod]
        public async Task StatusCode200LogInformation()
        {
            var highestSeverityLevel = LogSeverityLevel.None;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                });
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 200);
            var options = new NexusLinkMiddlewareOptions();
            options.Features.LogRequestAndResponse.Enabled = true;
            var outerHandler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(innerHandler.InvokeAsync, options);
            var context = new DefaultHttpContext();
            context.SetRequest(url);

            await outerHandler.InvokeAsync(context);

            Assert.AreEqual(LogSeverityLevel.Information, highestSeverityLevel);
        }

        /// <summary>
        /// LogRequestAndResponse must not come before BatchLogs in the pipe
        /// </summary>
        [TestMethod]
        public async Task StatusCode400LogWarning()
        {
            var highestSeverityLevel = LogSeverityLevel.None;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger.Setup(logger =>
                    logger.LogSync(
                        It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 400);
            var options = new NexusLinkMiddlewareOptions();
            options.Features.LogRequestAndResponse.Enabled = true;
            var outerHandler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(innerHandler.InvokeAsync, options);
            var context = new DefaultHttpContext();
            context.SetRequest(url);

            await outerHandler.InvokeAsync(context);

            Assert.AreEqual(LogSeverityLevel.Warning, highestSeverityLevel);
        }

        [TestMethod]
        public async Task StatusCode500LogError()
        {
            var highestSeverityLevel = LogSeverityLevel.None;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger.Setup(logger =>
                    logger.LogSync(
                        It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 500);
            var options = new NexusLinkMiddlewareOptions();
            options.Features.LogRequestAndResponse.Enabled = true;
            var outerHandler = new Misc.AspNet.Sdk.Inbound.NexusLinkMiddleware(innerHandler.InvokeAsync, options);
            var context = new DefaultHttpContext();
            context.SetRequest(url);

            await outerHandler.InvokeAsync(context);

            Assert.AreEqual(LogSeverityLevel.Error, highestSeverityLevel);
        }

        private class ReturnResponseWithPresetStatusCode
        {
            private readonly int _statusCode;
            private readonly RequestDelegate _next;

            public ReturnResponseWithPresetStatusCode(RequestDelegate next, int statusCode)
            {
                _next = next;
                _statusCode = statusCode;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                context.Response.StatusCode = _statusCode;
                FulcrumException fulcrumException = null;
                if (_statusCode >= 500)
                {
                    fulcrumException = new FulcrumAssertionFailedException("Internal error message");
                }
                else if (_statusCode >= 400)
                {
                    fulcrumException = new FulcrumServiceContractException("Client error message");
                }

                if (_statusCode >= 400)
                {
                    await context.Response.WriteAsync("Test");
                    context.Response.Body = new MemoryStream();
                    context.Response.ContentType = "application/json";
                    var fulcrumError = ExceptionConverter.ToFulcrumError(fulcrumException);
                    var content = ExceptionConverter.ToJsonString(fulcrumError, Formatting.Indented);
                    await context.Response.WriteAsync(content);
                }
            }
        }
    }
}
#endif