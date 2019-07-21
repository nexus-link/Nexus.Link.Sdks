namespace Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication
{
    /// <summary>
    /// The services
    /// </summary>
    public interface IAuthenticationCapability
    {
        /// <summary>
        /// Service for tokens
        /// </summary>
        ITokenService TokenService { get; }
    }
}
