using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.BusinessEvents.Sdk.RestClients;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Startup;
using Nexus.Link.Services.Contracts.Capabilities;
using Nexus.Link.Services.Implementations.Adapter.Startup.Configuration;
using Nexus.Link.Services.Implementations.BusinessApi.Startup;
using Nexus.Link.Services.Contracts.Events;
using Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration;
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

        private readonly IDictionary<Type, IEnumerable<Type>> _capabilityInterfaceToControllerClasses = new Dictionary<Type, IEnumerable<Type>>();

        /// <inheritdoc/>
        protected NexusAdapterStartup(IConfiguration configuration) : base(configuration, false)
        {
            AdapterConfiguration = new AdapterConfiguration(configuration);
        }

        /// <summary>
        /// Replace the <see cref="StartupBase.DependencyInjectServices"/> with <see cref="DependencyInjectBusinessApiServices"/> and <see cref="DependencyInjectAdapterServices"/>.
        /// </summary>
        /// <param name="services"></param>
        protected override void DependencyInjectServices(IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            base.DependencyInjectServices(services, mvcBuilder);
            DependencyInjectBusinessApiServices(services);
            DependencyInjectAdapterServices(services);
            AddControllersToMvc(services, mvcBuilder);
            var subscriptionHandler = new EventSubscriptionHandler();
            AddSubscriptions(subscriptionHandler);
            services.AddSingleton<IEventReceiver>(new EventReceiverLogic(subscriptionHandler));
        }

        /// <summary>
        /// Register which controllers that should be used for a specific capability interface.
        /// </summary>
        /// <param name="capabilityInterface"></param>
        /// <param name="controllerTypes"></param>
        protected void RegisterCapabilityControllers(Type capabilityInterface, params Type[] controllerTypes)
        {
            InternalContract.Require(capabilityInterface, type => type.IsInterface, nameof(capabilityInterface));
            InternalContract.Require(capabilityInterface.IsInterface, 
                $"The parameter {nameof(capabilityInterface)} must be an interface.");
            InternalContract.Require(typeof(IServicesCapability).IsAssignableFrom(capabilityInterface), 
                $"The parameter {nameof(capabilityInterface)} must inherit from {typeof(IServicesCapability).FullName}.");
            foreach (var controllerType in controllerTypes)
            {
                InternalContract.Require(controllerType, type => type.IsClass, nameof(controllerType));
            }

            _capabilityInterfaceToControllerClasses.Add(capabilityInterface, controllerTypes);
        }

        private void AddControllersToMvc(IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            using (var serviceScope = services.BuildServiceProvider().CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;

                foreach (var serviceType in _capabilityInterfaceToControllerClasses.Keys)
                {
                    FulcrumAssert.IsTrue(serviceType.IsInterface);
                    var service = serviceProvider.GetService(serviceType);
                    if (service == null) continue;
                    var controllerTypes = _capabilityInterfaceToControllerClasses[serviceType];
                    FulcrumAssert.IsNotNull(controllerTypes);
                    foreach (var controllerType in controllerTypes)
                    {
                        FulcrumAssert.IsTrue(controllerType.IsClass);
                        var assembly = controllerType.GetTypeInfo().Assembly;
                        mvcBuilder.AddApplicationPart(assembly);
                    }
                }
            }
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
        protected abstract void AddSubscriptions(EventSubscriptionHandler subscriptionHandler);
    }
}
#endif