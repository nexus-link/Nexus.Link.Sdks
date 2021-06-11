using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Libraries.Web.AspNet.Authorize;
using Nexus.Link.Libraries.Web.AspNet.Startup;
using Nexus.Link.Services.Contracts.Events;
using Nexus.Link.Services.Implementations.Common.Events;

namespace Nexus.Link.Services.Implementations.Common.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public abstract class NexusCommonStartup : StartupBase
    {
        /// <inheritdoc/>
        protected NexusCommonStartup(IConfiguration configuration, bool isBusinessApi) 
            : base(configuration, isBusinessApi)
        {
        }

        #region Configure Services
        /// <inheritdoc />
        protected override void DependencyInjectServices(IServiceCollection services)
        {
            // Authenticate by tokens
            services
                .AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer();
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("HasMandatoryRole", 
                    policy => policy.Requirements.Add(new MandatoryRoleRequirement()));
            });
            SetMandatoryRole(services);
        }

        /// <inheritdoc />
        protected override void DependencyInjectServicesAdvanced(IServiceCollection services, IServiceProvider serviceProvider)
        {
            var subscriptionHandler = new EventSubscriptionHandler();
            AddSubscriptions(subscriptionHandler, serviceProvider);
            if (subscriptionHandler.HasSubscriptions)
            {
                services.AddSingleton<IEventReceiver>(new EventReceiverLogic(subscriptionHandler));
            }
        }

        /// <summary>
        /// This is where the adapter can add events that it wants to subscribe to.
        /// </summary>
        /// <param name="subscriptionHandler">Use this to add subscriptions</param>
        /// <param name="serviceProvider"></param>
        protected abstract void AddSubscriptions(EventSubscriptionHandler subscriptionHandler, IServiceProvider serviceProvider);


        /// <summary>
        /// Set the role that is mandatory for calls to this app.
        /// </summary>
        /// <param name="services"></param>
        protected abstract void SetMandatoryRole(IServiceCollection services);
        #endregion

        #region Configure
        #endregion
    }
}
