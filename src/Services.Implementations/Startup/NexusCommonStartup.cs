using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Authorize;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.AppSupport;
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration;
using StartupBase = Nexus.Link.Libraries.Web.AspNet.Startup.StartupBase;
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
        /// Call <see cref="MandRR"/>
        /// </summary>
        /// <param name="services"></param>
        protected abstract void SetMandatoryRole(IServiceCollection services);
        #endregion

        #region Configure
        #endregion
    }
}
#endif