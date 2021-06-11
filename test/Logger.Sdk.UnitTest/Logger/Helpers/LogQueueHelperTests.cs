using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nexus.Link.Configurations.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Logger.Sdk;
using Nexus.Link.Logger.Sdk.Helpers;
using Nexus.Link.Logger.Sdk.RestClients;
using System.Threading.Tasks;

namespace Logger.Sdk.UnitTest.Logger.Helpers
{
    [TestClass]
    public class LogQueueHelperTests
    {
        private Mock<ILogClient> _legacyLoggerMock;

        private Mock<ILogQueueHelper<LogMessage>> LogQueueHelperMock { get; set; }
        private Mock<ILeverServiceConfiguration> LoggingServiceConfigurationMock { get; set; }

        private LogQueueHelper<string> SystemUnderTest { get; set; }

        private static readonly Tenant Tenant = new Tenant("mstest", "local");

        [TestInitialize]
        public void RunBeforeEachTest()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(LogQueueHelperTests).FullName);

            _legacyLoggerMock = new Mock<ILogClient>();
            LoggingServiceConfigurationMock = new Mock<ILeverServiceConfiguration>();

            // To setup Tenant on value provider
            // ReSharper disable once ObjectCreationAsStatement
            FulcrumApplication.Context.ClientTenant = Tenant;
            SystemUnderTest = new LogQueueHelper<string>(LoggingServiceConfigurationMock.Object);
        }

        /// <summary>
        /// Given tenant configuration for virtual service 'Logging'
        /// When TryGetQueue{T}
        /// Then a new TruncatingAzureStorageQueue{T} is created
        /// </summary>
        [TestMethod]
        public async Task GetStorageQueue()
        {
            // Arrange
            var json = @"{""LoggerConnectionString"":""usedevstorage=true"",""QueueName"":""queuefortest""}";
            var config = new LeverConfiguration(Tenant, "Logging", JObject.Parse(json));

            LoggingServiceConfigurationMock.Setup(f => f.GetConfigurationForAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>())).ReturnsAsync(config);

            // Act
            var (hasStorageQueue, writableQueue) = await SystemUnderTest.TryGetQueueAsync(Tenant);

            // Assert
            Assert.IsTrue(hasStorageQueue,
                "Successful outcome");
            Assert.AreEqual(typeof(TruncatingAzureStorageQueue<string>), writableQueue.GetType(),
                "A storageQueue is returned");
        }

        /// <summary>
        /// Given tennantconfiguration missing LoggerConnectionString
        /// When TryGetQueue{T}
        /// Then fail
        /// </summary>
        [TestMethod]
        public async Task LoggerConnectionString_MandatoryForSuccess()
        {
            // Arrange
            var json = @"{""Key1"":""some value""}";
            var config = new LeverConfiguration(Tenant, "Logging", JObject.Parse(json));

            LoggingServiceConfigurationMock.Setup(f => f.GetConfigurationForAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>())).ReturnsAsync(config);

            // Act
            var result = await SystemUnderTest.TryGetQueueAsync(Tenant);

            // Assert
            Assert.IsFalse(result.HasStorageQueue,
                "No StorageQueue expected when LoggerConnectionString is missing");
        }

        /// <summary>
        /// Given tennantconfiguration missing QueueName
        /// When TryGetQueue{T}
        /// Then fail
        /// </summary>
        [TestMethod]
        public async Task QueueName_MandatoryForSuccess()
        {
            // Arrange
            var json = @"{""LoggerConnectionString"":""usedevstorage=true""}";
            var config = new LeverConfiguration(Tenant, "Logging", JObject.Parse(json));

            LoggingServiceConfigurationMock.Setup(f => f.GetConfigurationForAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>())).ReturnsAsync(config);

            // Act
            var result = await SystemUnderTest.TryGetQueueAsync(Tenant);

            // Assert
            Assert.IsFalse(result.HasStorageQueue,
                "No StorageQueue expected when QueueName is missing");
        }
    }
}