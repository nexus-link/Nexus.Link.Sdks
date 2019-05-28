using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Queue.Logic;
using Nexus.Link.Logger.Sdk;
using Nexus.Link.Logger.Sdk.Helpers;
using System;
using System.Threading.Tasks;

namespace Logger.Sdk.UnitTest.Logger.Helpers
{
    [TestClass]
    public class TruncatingAzureStorageQueueTests
    {
        private MemoryQueue<LogMessage> AzureQueueMock { get; set; }
        private TruncatingAzureStorageQueue<LogMessage> SystemUnderTest { get; set; }
        private Mock<IFallbackLogger> FallbackLoggerMock { get; set; }
        private static string LONG_STRING_65000_CHARS = new string('*', 65000);
        private static string MEDIUM_STRING_32000_CHARS = new string('X', 32000);

        [TestInitialize]
        public void RunBeforeEachTest()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TruncatingAzureStorageQueueTests).FullName);

            FallbackLoggerMock = new Mock<IFallbackLogger>();
            FulcrumApplication.Setup.FallbackLogger = FallbackLoggerMock.Object;

            AzureQueueMock = new MemoryQueue<LogMessage>(nameof(TruncatingAzureStorageQueueTests));
            SystemUnderTest = new TruncatingAzureStorageQueue<LogMessage>(AzureQueueMock);
        }

        [TestMethod]
        public async Task LogMessageOverflowIsSafeLoggedAndTruncatedOnPassToBase()
        {
            // Arrange
            var largeLogMessage = new LogMessage
            {
                Message = LONG_STRING_65000_CHARS + "REMOVED",
                Timestamp = DateTimeOffset.Now
            };

            // Act
            await SystemUnderTest.AddMessageAsync(largeLogMessage);

            // Assert
            var truncatedMessage = await AzureQueueMock.GetOneMessageNoBlockAsync();
            Assert.IsTrue(truncatedMessage.Message.EndsWith("*"),
                "Exceeding part of message is trimmed");

            FallbackLoggerMock.Verify(f => f.SafeLog(LogSeverityLevel.Warning, It.IsAny<string>()), Times.Once,
                "Expected the original message to be safelogged");

            FallbackLoggerMock.Verify(f => f.SafeLog(LogSeverityLevel.Critical, It.IsAny<string>()), Times.Once,
                "A log message was over 65000 characters, so the text message part in the logmessage was truncated.");
        }

        [TestMethod]
        public async Task ShortLogMessagePassedToBase()
        {
            // Arrange
            var largeLogMessage = new LogMessage
            {
                Message = MEDIUM_STRING_32000_CHARS + "THE-END",
                Timestamp = DateTimeOffset.Now
            };

            // Act
            await SystemUnderTest.AddMessageAsync(largeLogMessage);

            // Assert
            var storedMessage = await AzureQueueMock.GetOneMessageNoBlockAsync();
            Assert.IsTrue(storedMessage.Message.EndsWith("THE-END"),
                "Trailing part of message remains");

            FallbackLoggerMock.Verify(f => f.SafeLog(LogSeverityLevel.Warning, It.IsAny<string>()), Times.Never,
                "Expected the FallbackLogger to not have been involved");
        }
    }
}