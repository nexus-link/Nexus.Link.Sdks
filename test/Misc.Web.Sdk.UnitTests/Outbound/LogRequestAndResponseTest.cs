using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Misc.Web.Sdk.Outbound;
using Nexus.Link.Misc.Web.Sdk.Outbound.Options;

namespace Misc.Web.Sdk.UnitTests.Outbound
{
    [TestClass]
    public class LogRequestAndResponseTest : ISyncLogger
    {
        private readonly Dictionary<LogSeverityLevel, string> _lastMessageDictionary =
            new Dictionary<LogSeverityLevel, string>();

        private int _numberOfLogs;
        private int _expectedNumberOfLogs;
        private NexusLinkHandlerOptions _options;
        private NexusLinkHandler _handler;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(LogRequestAndResponseTest).FullName);
            FulcrumApplication.Setup.SynchronousFastLogger = this;
            _options = new NexusLinkHandlerOptions
            {
                Features = new HandlerFeatures
                {
                    LogRequestAndResponse = {Enabled = true}
                }
            };
            _handler = new NexusLinkHandler(_options);
        }

        [TestMethod]
        public async Task ResponseOk()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/okresponse");
            SetExpectedNumberOfLogs(1);
            await _handler.TestSendAsync(request, SendAsyncResponseOk);
            var lastMessage = LastMessage(LogSeverityLevel.Information);
            Assert.IsNotNull(lastMessage);
            Assert.IsTrue(lastMessage.Contains($"OUTBOUND request-response POST {request.RequestUri}"));
            Assert.IsTrue(lastMessage.Contains(request.RequestUri.ToString()));
        }

        [TestMethod]
        public async Task ResponseBadRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/badrequest");
            SetExpectedNumberOfLogs(1);
            await _handler.TestSendAsync(request, SendAsyncResponseBadRequest);
            var lastMessage = LastMessage(LogSeverityLevel.Error);
            Assert.IsNotNull(lastMessage);
            Assert.IsTrue(lastMessage.Contains($"OUTBOUND request-response POST {request.RequestUri}"));
        }

        [TestMethod]
        public async Task ResponseException()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/exception");
            SetExpectedNumberOfLogs(1);
            try
            {
                await _handler.TestSendAsync(request, SendAsyncResponseException);
                Assert.Fail("Expected an exception");
            }
            catch (Exception)
            {
                var lastMessage = LastMessage(LogSeverityLevel.Error);
                Assert.IsNotNull(lastMessage);
                Assert.IsTrue(lastMessage.Contains($"OUTBOUND request-exception POST {request.RequestUri}"));
            }
        }

        private void SetExpectedNumberOfLogs(int expectedNumberOfLogs)
        {
            _expectedNumberOfLogs = expectedNumberOfLogs;
            _numberOfLogs = 0;
        }

        private string LastMessage(LogSeverityLevel severityLevel)
        {
            var count = 0;
            while (count++ < 100 && _numberOfLogs < _expectedNumberOfLogs) Thread.Sleep(TimeSpan.FromMilliseconds(10));
            Assert.IsFalse(_numberOfLogs < _expectedNumberOfLogs,
                $"Expected {_expectedNumberOfLogs} logs, got {_numberOfLogs}");
            while (count++ < 100 && _numberOfLogs <= _expectedNumberOfLogs &&
                   !_lastMessageDictionary.ContainsKey(severityLevel)) Thread.Sleep(TimeSpan.FromMilliseconds(10));
            Assert.IsFalse(_numberOfLogs > _expectedNumberOfLogs,
                $"Expected {_expectedNumberOfLogs} logs, got {_numberOfLogs}");
            return !_lastMessageDictionary.ContainsKey(severityLevel) ? null : _lastMessageDictionary[severityLevel];
        }

        private static async Task<HttpResponseMessage> SendAsyncResponseOk(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) {RequestMessage = request};
            return await Task.FromResult(response);
        }

        private static async Task<HttpResponseMessage> SendAsyncResponseBadRequest(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {RequestMessage = request};
            return await Task.FromResult(response);
        }

        private static Task<HttpResponseMessage> SendAsyncResponseException(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new ApplicationException("SendAsync failed");
        }

        public void Log(LogSeverityLevel logSeverityLevel, string message)
        {
            throw new NotImplementedException();
        }

        public void LogSync(LogRecord logRecord)
        {
            Console.WriteLine($"\r{logRecord.ToLogString(true)}\r");
            _lastMessageDictionary[logRecord.SeverityLevel] = logRecord.Message;
            _numberOfLogs++;
        }
    }
}
