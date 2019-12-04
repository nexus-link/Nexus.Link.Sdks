using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.AppSupport
{
    /// <inheritdoc />
    public class AppSupportCapability : IAppSupportCapability
    {
        /// <inheritdoc />
        public AppSupportCapability(IAsyncLogger logger, IHttpSender httpSender)
        {
            LoggingService = new LoggingLogic(logger);
            ConfigurationService = new ConfigurationLogic(
                httpSender.CreateHttpSender($"api/v1/{FulcrumApplication.Setup.Tenant.Organization}/{FulcrumApplication.Setup.Tenant.Environment}"));
        }

        /// <inheritdoc />
        public IConfigurationService ConfigurationService { get; }

        /// <inheritdoc />
        public ILoggingService LoggingService { get; }
    }
}
