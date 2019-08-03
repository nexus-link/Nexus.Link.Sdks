using StartupBase = Nexus.Link.Libraries.Web.AspNet.Startup.StartupBase;
#if NETCOREAPP
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Link.Services.Implementations.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public abstract class NexusAdapterStartup : StartupBase
    {

        /// <inheritdoc/>
        protected NexusAdapterStartup(IConfiguration configuration) : base(configuration, false)
        {
        }

        /// <summary>
        /// Replace the <see cref="StartupBase.DependencyInjectServices"/> with <see cref="DependencyInjectBusinessApiServices"/> and <see cref="DependencyInjectAdapterServices"/>.
        /// </summary>
        /// <param name="services"></param>
        protected override void DependencyInjectServices(IServiceCollection services)
        {
            DependencyInjectBusinessApiServices(services);
            DependencyInjectAdapterServices(services);
        }

        /// <summary>
        /// This is where the business API injects its own services.
        /// </summary>
        /// <param name="services">From the parameter to Startup.ConfigureServices.</param>
        /// <remarks>Always override this to inject your services.</remarks>
        protected abstract void DependencyInjectBusinessApiServices(IServiceCollection services);

        /// <summary>
        /// This is where the adapter injects its own services.
        /// </summary>
        /// <param name="services">From the parameter to Startup.ConfigureServices.</param>
        /// <remarks>Always override this to inject your services.</remarks>
        protected abstract void DependencyInjectAdapterServices(IServiceCollection services);
    }
}
#endif