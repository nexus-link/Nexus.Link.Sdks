namespace Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport
{
    /// <summary>
    /// Application support
    /// </summary>
    public interface IAppSupportCapability : Libraries.Core.Platform.Services.IControllerInjector
    {
        /// <summary>
        /// Service for getting application configurations
        /// </summary>
        IConfigurationService ConfigurationService { get; }

        /// <summary>
        /// Service for logging messages
        /// </summary>
        ILoggingService LoggingService { get; }
    }
}