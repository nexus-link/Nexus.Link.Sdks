using System;

namespace Nexus.Link.Services.Contracts.Capabilities
{
    /// <summary>
    /// If an interface that implements this has a dependency injection, then controllers can be injected.
    /// </summary>
    [Obsolete("Please use Nexus.Link.Libraries.Core.Platform.Services.IControllerInjector. Obsolete since 2019-12-18.")]
    public interface IControllerInjector : Libraries.Core.Platform.Services.IControllerInjector
    {
    }
}