using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.AppSupport
{
    /// <inheritdoc />
    public class AppSupportCapability : IAppSupportCapability
    {
        
        /// <inheritdoc />
        public AppSupportCapability(IHttpSender httpSender)
        {
            if (FulcrumApplication.IsInDevelopment)
            {

            }
            else
            {
                LoggingService = new LoggingService_NotImplemented();
                ConfigurationService = new ConfigurationRestService(httpSender.CreateHttpSender("Configurations"));
            }
        }

        /// <inheritdoc />
        public IConfigurationService ConfigurationService { get; }

        /// <inheritdoc />
        public ILoggingService LoggingService { get; }

        // ReSharper disable once InconsistentNaming
        private class LoggingService_NotImplemented : ILoggingService
        {
            public LoggingService_NotImplemented()
            {
            }

            /// <inheritdoc />
            public Task LogAsync(JToken message, CancellationToken token = new CancellationToken())
            {
                throw new FulcrumNotImplementedException();
            }
        }
    }
}
