namespace Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication
{
    /// <summary>
    /// Authentication
    /// </summary>
    public interface IAuthenticationCapability : IControllerInjector
    {
        /// <summary>
        /// Service for tokens
        /// </summary>
        ITokenService TokenService { get; }
    }
}
