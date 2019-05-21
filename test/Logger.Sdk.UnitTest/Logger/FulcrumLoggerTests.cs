using System;
using System.Threading.Tasks;
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

namespace Logger.Sdk.UnitTest.Logger
{
    [TestClass]
    public class FulcrumLoggerTests
    {
        private Mock<ILogClient> _legacyLoggerMock;

        private Mock<ILogQueueHelper<LogMessage>> LogQueueHelperMock { get; set; }

        private FulcrumLogger SystemUnderTest { get; set; }

        private static readonly Tenant Tenant = new Tenant("org", "env");

        [TestInitialize]
        public void RunBeforeEachTest()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(SdkTests).FullName);

            _legacyLoggerMock = new Mock<ILogClient>();
            LogQueueHelperMock = new Mock<ILogQueueHelper<LogMessage>>();

            // To setup Tenant on value provider
            // ReSharper disable once ObjectCreationAsStatement
            FulcrumApplication.Context.ClientTenant = Tenant;
            SystemUnderTest = new FulcrumLogger(LogQueueHelperMock.Object, _legacyLoggerMock.Object);
        }

        [TestMethod]
        public async Task Given_ServiceConfigReturnsASQConfig_When_LogAsync_Then_LogmessagePushedToStorageQueue()
        {
            // Arrange
            IWritableQueue<LogMessage> storageQueue = new MemoryQueue<LogMessage>("MSTestMemQueue");
            LogQueueHelperMock.Setup(f => f.TryGetQueue(It.IsAny<Tenant>(), out storageQueue)).Returns(true);

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
    }
}