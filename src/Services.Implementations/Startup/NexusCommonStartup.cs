using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Libraries.Web.AspNet.Authorize;
using Nexus.Link.Libraries.Web.AspNet.Startup;
using Nexus.Link.Services.Contracts.Capabilities;

#if NETCOREAPP

namespace Nexus.Link.Services.Implementations.BusinessApi.Startup
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
#endif