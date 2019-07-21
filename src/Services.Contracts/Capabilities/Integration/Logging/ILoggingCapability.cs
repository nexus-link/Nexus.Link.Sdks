namespace Nexus.Link.Services.Contracts.Capabilities.Integration.Logging
{
    /// <summary>
    /// The services 
    /// </summary>
    public interface ILoggingCapability
    {
        /// <summary>
        /// Service for business events
        /// </summary>
        ILoggingService LoggingService { get; }
    }
}
