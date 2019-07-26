#if NETCOREAPP
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.AspNet.Startup;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Implementations.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Implementations.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public abstract class ServicesStartupHelperBase : StartupHelperBase
    {
        /// <inheritdoc/>
        protected ServicesStartupHelperBase(IConfiguration configuration, bool isBusinessApi)
        :base(configuration, isBusinessApi)
        {
        }

        #region Configure Services
        /// <inheritdoc/>
        protected override void InitialLocalConfiguration(IServiceCollection services)
        {
            if (IsBusinessApi)
            {
                NexusLinkAuthenticationBaseUrl = FulcrumApplication.AppSettings.GetString("Nexus.AuthenticationUrl", true);
                BusinessEventsBaseUrl =
                    FulcrumApplication.AppSettings.GetString("Nexus.BusinessEventsUrl", true);
                // NexusAuthenticationManager = new NexusAuthenticationManager(FulcrumApplication.Setup.Tenant, NexusAuthenticationBaseUrl);
                NexusLinkTokenRefresher = CreateNexusTokenRefresher();
            }

            LocalAuthenticationBaseUrl = FulcrumApplication.AppSettings.GetString("Local.AuthenticationUrl", true);
            LocalTokenRefresher = CreateLocalTokenRefresher();
        }

        /// <summary>
        /// Creates a <see cref="ITokenRefresherWithServiceClient"/>.
        /// </summary>
        /// <remarks>
        /// Gets the credentials from app settings (Nexus.ClientId, Nexus.ClientSecret)  and calls a token service at <see cref="StartupHelperBase.NexusLinkAuthenticationBaseUrl"/>."/>
        /// </remarks>
        protected virtual ITokenRefresherWithServiceClient CreateNexusTokenRefresher()
        {
            var clientId = FulcrumApplication.AppSettings.GetString("Nexus.ClientId", true);
            var clientSecret = FulcrumApplication.AppSettings.GetString("Nexus.ClientSecret", true);
            var credentials = new AuthenticationCredentials { ClientId = clientId, ClientSecret = clientSecret };
            var tokenRefresher =
                NexusAuthenticationManager.CreateTokenRefresher(FulcrumApplication.Setup.Tenant, NexusLinkAuthenticationBaseUrl, credentials);
            return tokenRefresher;
        }

        /// <summary>
        /// Creates a <see cref="ITokenRefresherWithServiceClient"/>.
        /// </summary>
        /// <remarks>
        /// Gets the credentials from app settings (Local.ClientId, Local.ClientSecret) and calls a token service at <see cref="StartupHelperBase.NexusLinkAuthenticationBaseUrl"/>."/>
        /// </remarks>
        private static ITokenRefresherWithServiceClient CreateLocalTokenRefresher()
        {
            var clientId = FulcrumApplication.AppSettings.GetString("Local.ClientId", true);
            var clientSecret = FulcrumApplication.AppSettings.GetString("Local.ClientSecret", true);
            var credentials = new AuthenticationCredentials { ClientId = clientId, ClientSecret = clientSecret };
            var tokenRefresher =
                AuthenticationManager.CreateTokenRefresher(FulcrumApplication.Setup.Tenant, LocalAuthenticationBaseUrl, credentials);
            return tokenRefresher;
        }

        /// <inheritdoc/>
        protected override void DependencyInjectNexusServices(IServiceCollection services)
        {
            services.AddScoped<IAuthenticationCapability>(provider =>
                ValidateDependencyInjection(provider,
                    p => new AuthenticationCapability(NexusLinkAuthenticationBaseUrl, NexusLinkTokenRefresher.GetServiceClient())));
            services.AddScoped<IBusinessEventsCapability>(provider =>
                new BusinessEventsCapability(BusinessEventsBaseUrl, NexusLinkTokenRefresher.GetServiceClient()));
            services
                .AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer();
        }
        #endregion

        #region Configure

        /// <inheritdoc/>
        protected override void ConfigureNexusLinkMiddleware(IApplicationBuilder app, IHostingEnvironment env)
        {
            base.ConfigureNexusLinkMiddleware(app, env);

            // Verify tokens with our public key
            var rsaPublicKey = AuthenticationManager
                .GetPublicRsaKeyAsync(FulcrumApplication.Setup.Tenant, LocalAuthenticationBaseUrl).Result;
            app.UseNexusTokenValidationHandler(rsaPublicKey);
        }
        #endregion
    }
}
#endif