
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.BusinessEvents;
#if NETCOREAPP
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.AspNet.Startup;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;

namespace Nexus.Link.Services.Implementations.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public abstract class ServicesStartupHelperBase : StartupHelperBase
    {
        /// <summary>
        /// The base URL to the authentication service for authenticating your app vs. Nexus Link.
        /// </summary>
        /// <remarks>Only useful for the Business API, not for Nexus Adapters.</remarks>
        protected string NexusLinkAuthenticationBaseUrl { get; set; }

        /// <summary>
        /// The base URL to the Nexus Link business events service.
        /// </summary>
        /// <remarks>Only useful for the Business API, not for Nexus Adapters.</remarks>
        protected string BusinessEventsBaseUrl { get; set; }

        /// <summary>
        /// A token generator for authenticating your app vs. Nexus Link.
        /// </summary>
        /// <remarks>Only useful for the Business API, not for Nexus Adapters.</remarks>
        protected ITokenRefresherWithServiceClient NexusLinkTokenRefresher { get; set; }

        /// <summary>
        /// The base URL to the authentication service for authenticating within your platform.
        /// </summary>
        protected string LocalAuthenticationBaseUrl { get; set; }

        /// <summary>
        /// A token generator for authenticating between adapters and the business API.
        /// </summary>
        protected ITokenRefresherWithServiceClient LocalTokenRefresher { get; set; }

        /// <inheritdoc/>
        protected ServicesStartupHelperBase(IConfiguration configuration, bool isBusinessApi)
        :base(configuration, isBusinessApi)
        {
        }

        #region Configure Services
        /// <inheritdoc/>
        protected override void InitialLocalConfiguration(IServiceCollection services)
        {
            base.InitialLocalConfiguration(services);
            if (!IsBusinessApi) return;

            NexusLinkAuthenticationBaseUrl = FulcrumApplication.AppSettings.GetString("Nexus.AuthenticationUrl", true);
            BusinessEventsBaseUrl =
                FulcrumApplication.AppSettings.GetString("Nexus.BusinessEventsUrl", true);
            NexusLinkTokenRefresher = CreateNexusTokenRefresher();

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
        protected virtual ITokenRefresherWithServiceClient CreateLocalTokenRefresher()
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
                ValidateDependencyInjection(provider, p =>
                new BusinessEventsCapability(BusinessEventsBaseUrl, NexusLinkTokenRefresher.GetServiceClient())));
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
            if (!IsBusinessApi) return;

            // Verify tokens with our public key
            var rsaPublicKey = AuthenticationManager
                .GetPublicRsaKeyAsync(FulcrumApplication.Setup.Tenant, LocalAuthenticationBaseUrl).Result;
            app.UseNexusTokenValidationHandler(rsaPublicKey);
        }
        #endregion

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            if (!IsBusinessApi) return;
            FulcrumValidate.IsNotNullOrWhiteSpace(NexusLinkAuthenticationBaseUrl, nameof(NexusLinkAuthenticationBaseUrl), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(BusinessEventsBaseUrl, nameof(BusinessEventsBaseUrl), errorLocation);
            FulcrumValidate.IsNotNull(NexusLinkTokenRefresher, nameof(NexusLinkTokenRefresher), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(LocalAuthenticationBaseUrl, nameof(LocalAuthenticationBaseUrl), errorLocation);
            FulcrumValidate.IsNotNull(LocalTokenRefresher, nameof(LocalTokenRefresher), errorLocation);
        }
    }
}
#endif