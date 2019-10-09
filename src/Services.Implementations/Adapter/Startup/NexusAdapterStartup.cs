using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Libraries.Web.AspNet.Startup;
using Nexus.Link.Services.Implementations.Adapter.Startup.Configuration;
using Nexus.Link.Services.Implementations.BusinessApi.Startup;
using Nexus.Link.Services.Contracts.Events;
using Nexus.Link.Services.Implementations.Adapter.Events;

#if NETCOREAPP

namespace Nexus.Link.Services.Implementations.Adapter.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public abstract class NexusAdapterStartup : NexusCommonStartup
    {

        /// <summary>
        /// Access to all relevant configuration
        /// </summary>
        protected AdapterConfiguration AdapterConfiguration { get; }

        /// <inheritdoc/>
        protected NexusAdapterStartup(IConfiguration configuration) : base(configuration, false)
        {
            AdapterConfiguration = new AdapterConfiguration(configuration);
        }

        /// <summary>
        /// Replace the <see cref="StartupBase.DependencyInjectServices"/> with <see cref="DependencyInjectBusinessApiServices"/> and <see cref="DependencyInjectAdapterServices"/>.
        /// </summary>
        protected override void DependencyInjectServices(IServiceCollection services)
        {
            base.DependencyInjectServices(services);
            DependencyInjectBusinessApiServices(services);
            DependencyInjectAdapterServices(services);
        }

        /// <inheritdoc />
        protected override void DependencyInjectServicesAdvanced(IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            var subscriptionHandler = new EventSubscriptionHandler();
            AddSubscriptions(subscriptionHandler, mvcBuilder);
            services.AddSingleton<IEventReceiver>(new EventReceiverLogic(subscriptionHandler));
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

        /// <summary>
        /// This is where the adapter can add events that it wants to subscribe to.
        /// </summary>
        /// <param name="subscriptionHandler">Use this to add subscriptions</param>
        /// <param name="mvcBuilder"></param>
        protected abstract void AddSubscriptions(EventSubscriptionHandler subscriptionHandler, IMvcBuilder mvcBuilder);
    }
}
#endif