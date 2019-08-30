using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Queue.Logic;
using Nexus.Link.Libraries.Core.Queue.Model;
using Nexus.Link.Logger.Sdk;
using Nexus.Link.Logger.Sdk.Helpers;
using Nexus.Link.Logger.Sdk.RestClients;
using System;
using System.Threading.Tasks;

namespace Logger.Sdk.UnitTest.Logger
{
    [TestClass]
    public class FulcrumLoggerTests
    {
        private Mock<ILogClient> LegacyLoggerMock { get; set; }

        private Mock<ILogQueueHelper<LogMessage>> LogQueueHelperMock { get; set; }

        private FulcrumLogger SystemUnderTest { get; set; }

        private static readonly Tenant Tenant = new Tenant("org", "env");

        [TestInitialize]
        public void RunBeforeEachTest()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(SdkTests).FullName);

            LegacyLoggerMock = new Mock<ILogClient>();
            LogQueueHelperMock = new Mock<ILogQueueHelper<LogMessage>>();

            // To setup Tenant on value provider
            // ReSharper disable once ObjectCreationAsStatement
            FulcrumApplication.Context.ClientTenant = Tenant;
            SystemUnderTest = new FulcrumLogger( LegacyLoggerMock.Object, LogQueueHelperMock.Object);
        }

        /// <summary>
        /// Given Tenantconfiguration for virtual service Logging
        /// When LogAsync
        /// Then Logrecord is transformed and pushed to a WritableQueue<LogMessage>
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FulcrumLoggerConvertLogrecordAndPushToWritableQueue()
        {
            // Arrange
            IWritableQueue<LogMessage> storageQueue = new MemoryQueue<LogMessage>("MSTestMemQueue");
            LogQueueHelperMock
                .Setup(f => f.TryGetQueueAsync(It.IsAny<Tenant>()))
                .Returns(Task<(bool, IWritableQueue<LogMessage>)>.FromResult((true, storageQueue)));

            var logRecord = new LogRecord
            {
                Message = "Message for queue",
                SeverityLevel = LogSeverityLevel.Information,
                TimeStamp = DateTimeOffset.Now
            };

            // Act
            await SystemUnderTest.LogAsync(logRecord);

            // Assert
            var logMessage = await (storageQueue as IReadableQueue<LogMessage>).GetOneMessageNoBlockAsync();
            Assert.AreEqual(logRecord.Message, logMessage.Message,
                "Expect message pushed to persistent storage");
        }

        /// <summary>
        /// Given Tenant not configured for virtual service Logging
        /// When LogAsync
        /// Then Logrecord is transformed and pushed to the fallback restclient logger for fundamentals
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task LogMessageIsPushedToFundamentals()
        {
            // Arrange
            var logMessageSpy = new LogMessage();
            LegacyLoggerMock
                .Setup(f => f.LogAsync(It.IsAny<Tenant>(), It.IsAny<LogMessage[]>()))
                .Callback<Tenant, LogMessage[]>((tenant, logMessage) => logMessageSpy = logMessage[0]);

            LogQueueHelperMock
                .Setup(f => f.TryGetQueueAsync(It.IsAny<Tenant>()))
                .Returns(Task<(bool, IWritableQueue<LogMessage>)>.FromResult((false, default(IWritableQueue<LogMessage>))));

            var logRecord = new LogRecord
            {
                Message = "Message for fundmentals",
                SeverityLevel = LogSeverityLevel.Warning,
                TimeStamp = DateTimeOffset.Now
            };

            // Act
            await SystemUnderTest.LogAsync(logRecord);

            // Assert
            Assert.AreEqual(logRecord.Message, logMessageSpy.Message,
                "Expect message pushed to fundamentals");
        }
    }
}