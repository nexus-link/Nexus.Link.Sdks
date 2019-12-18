namespace Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication
{
    /// <summary>
    /// Authentication
    /// </summary>
    public interface IAuthenticationCapability : Libraries.Core.Platform.Services.IControllerInjector
    {
        /// <summary>
        /// Service for tokens
        /// </summary>
        ITokenService TokenService { get; }
    }
}
