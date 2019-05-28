using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Logger.Sdk;
using Nexus.Link.Logger.Sdk.RestClients;

namespace Logger.Sdk.UnitTest.Logger
{
    [TestClass]
    public class SdkTests
    {
        private Mock<ILogClient> _logClientMock;

        private static readonly Tenant Tenant = new Tenant("org", "env");

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(SdkTests).FullName);

            _logClientMock = new Mock<ILogClient>();

            var config = new Mock<ILeverConfiguration>();
            config.Setup(x => x.Value<string>("UseMemoryQueue")).Returns("true");

            // To setup Tenant on value provider
            // ReSharper disable once ObjectCreationAsStatement
            FulcrumApplication.Context.ClientTenant = Tenant;

            FulcrumApplication.Setup.SynchronousFastLogger = new FulcrumLogger(_logClientMock.Object);
        }

        [TestMethod]
        public void LogCriticalSuccess()
        {
            AssertStringLogMessage(LogSeverityLevel.Critical);
        }
        [TestMethod]
        public void LogErrorSuccess()
        {
            AssertStringLogMessage(LogSeverityLevel.Error);
        }

        [TestMethod]
        public void LogWarningSuccess()
        {
            AssertStringLogMessage(LogSeverityLevel.Warning);
        }

        [TestMethod]
        public void LogInformationSuccess()
        {
            AssertStringLogMessage(LogSeverityLevel.Information);
        }

        [TestMethod]
        public void LogVerboseSuccess()
        {
            AssertStringLogMessage(LogSeverityLevel.Verbose);
        }

        private void AssertStringLogMessage(LogSeverityLevel logSeverityLevel)
        {
            LogMessage loggedMessage = null;
            var logCalled = new ManualResetEvent(false);
            var correlationId = Guid.NewGuid().ToString();

            _logClientMock.Setup(mock => mock.LogAsync(It.IsAny<Tenant>(), It.IsAny<LogMessage[]>())).Returns(Task.CompletedTask).Callback<Tenant, LogMessage[]>(
                (tenant, msgs) =>
                {
                    var msg = msgs.FirstOrDefault(m => m.CorrelationId == correlationId);
                    if (msg == null) return;
                    loggedMessage = msg;
                    logCalled.Set();
                });
            // ReSharper disable once ObjectCreationAsStatement
            FulcrumApplication.Context.CorrelationId = correlationId;

            const string message = "msg in the c sprt";
            switch (logSeverityLevel)
            {
                case LogSeverityLevel.Verbose:
                    Log.LogVerbose(message);
                    break;
                case LogSeverityLevel.Information:
                    Log.LogInformation(message);
                    break;
                case LogSeverityLevel.Warning:
                    Log.LogWarning(message);
                    break;
                case LogSeverityLevel.Error:
                    Log.LogError(message);
                    break;
                case LogSeverityLevel.Critical:
                    Log.LogCritical(message);
                    break;
                default:
                    Assert.Fail($"Unexpected {nameof(logSeverityLevel)}: {logSeverityLevel}");
                    break;
            }

            Assert.IsTrue(logCalled.WaitOne(TimeSpan.FromSeconds(3)), "LogAsync was never called");
            Assert.IsNotNull(loggedMessage);
            Assert.AreEqual(typeof(SdkTests).FullName, loggedMessage.Originator);
            Assert.IsTrue(loggedMessage.Message.Contains(message));
            Assert.AreEqual(correlationId, loggedMessage.CorrelationId);
            Assert.AreEqual(logSeverityLevel, loggedMessage.SeverityLevel);
            Assert.IsNotNull(loggedMessage.Timestamp);
        }

        [TestMethod]
        public void LogExceptionSuccess()
        {
            LogMessage loggedMessage = null;
            var logCalled = new ManualResetEvent(false);

            _logClientMock.Setup(mock => mock.LogAsync(It.IsAny<Tenant>(), It.IsAny<LogMessage[]>())).Returns(Task.CompletedTask).Callback<Tenant, LogMessage[]>(
                (tenant, msgs) =>
                {
                    loggedMessage = msgs.FirstOrDefault();
                    logCalled.Set();
                });
            const string correlationId = "443D3DDA-BD04-4DDE-B5A3-65FFFBAC110D";
            // ReSharper disable once ObjectCreationAsStatement
            FulcrumApplication.Context.CorrelationId = correlationId;

            const string exceptionMessage = "The wrongdoings...";
            var exception = new Exception(exceptionMessage);
            Log.LogError("An eception", exception);

            Assert.IsTrue(logCalled.WaitOne(TimeSpan.FromSeconds(3)), "LogAsync was never called");
            Assert.IsNotNull(loggedMessage);
            Assert.AreEqual(typeof(SdkTests).FullName, loggedMessage.Originator);
            Assert.AreEqual(correlationId, loggedMessage.CorrelationId);
            Assert.AreEqual(LogSeverityLevel.Error, loggedMessage.SeverityLevel);
            Assert.IsNotNull(loggedMessage.Timestamp);
            Assert.IsTrue(loggedMessage.Message.Contains(exceptionMessage));
        }

        [TestMethod]
        public void LogWithNoClientTenant()
        {
            var logRecord = new LogRecord
            {
                Exception = null,
                Message = "Message text",
                SeverityLevel = LogSeverityLevel.Critical,
                StackTrace = null,
                TimeStamp = DateTimeOffset.Now
            };
            FulcrumApplication.Setup.SynchronousFastLogger.LogSync(logRecord);
        }
    }
}