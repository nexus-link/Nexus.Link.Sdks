
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StartupBase = Nexus.Link.Libraries.Web.AspNet.Startup.StartupBase;
#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Libraries.Web.AspNet.Startup;

namespace Nexus.Link.Services.Implementations.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public abstract class NexusBusinessApiStartup : StartupBase
    {

        /// <summary>
        /// A token generator for authenticating between adapters and the business API.
        /// </summary>
        private ITokenRefresherWithServiceClient _localTokenRefresher;

        /// <summary>
        /// A token generator for authenticating your app vs. Nexus Link.
        /// </summary>
        /// <remarks>Only useful for the Business API, not for Nexus Adapters.</remarks>
        private ITokenRefresherWithServiceClient _nexusLinkTokenRefresher;

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
        /// The base URL to the authentication service for authenticating within your platform.
        /// </summary>
        protected string LocalAuthenticationBaseUrl { get; set; }

        /// <summary>
        /// Credentials for calling Nexus Link services
        /// </summary>
        protected ServiceClientCredentials GetNexusCredentials() => _nexusLinkTokenRefresher.GetServiceClient();

        /// <summary>
        /// Credentials for calling adapters
        /// </summary>
        protected ServiceClientCredentials GetLocalCredentials()=> _localTokenRefresher.GetServiceClient();

        /// <inheritdoc/>
        protected NexusBusinessApiStartup(IConfiguration configuration) : base(configuration, true)
        {
        }

        #region Configure Services
        /// <inheritdoc />
        protected override void ConfigureServicesInitialUrgentPart(IServiceCollection services)
        {
            base.ConfigureServicesInitialUrgentPart(services);

            NexusLinkAuthenticationBaseUrl = FulcrumApplication.AppSettings.GetString("Nexus.AuthenticationUrl", true);
            BusinessEventsBaseUrl =
                FulcrumApplication.AppSettings.GetString("Nexus.BusinessEventsUrl", true);
            _nexusLinkTokenRefresher = CreateNexusTokenRefresher();

            LocalAuthenticationBaseUrl = FulcrumApplication.AppSettings.GetString("Local.AuthenticationUrl", true);
            _localTokenRefresher = CreateLocalTokenRefresher();
        }


        /// <inheritdoc />
        protected override void DependencyInjectServices(IServiceCollection services)
        {
            // Authenticate by tokens
            services
                .AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer();
        }

        /// <summary>
        /// Creates a <see cref="ITokenRefresherWithServiceClient"/>.
        /// </summary>
        /// <remarks>
        /// Gets the credentials from app settings (Nexus.ClientId, Nexus.ClientSecret)  and calls a token service at <see cref="NexusLinkAuthenticationBaseUrl"/>."/>
        /// </remarks>
        private ITokenRefresherWithServiceClient CreateNexusTokenRefresher()
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
        /// Gets the credentials from app settings (Local.ClientId, Local.ClientSecret) and calls a token service at <see cref="NexusLinkAuthenticationBaseUrl"/>."/>
        /// </remarks>
        private ITokenRefresherWithServiceClient CreateLocalTokenRefresher()
        {
            var clientId = FulcrumApplication.AppSettings.GetString("Local.ClientId", true);
            var clientSecret = FulcrumApplication.AppSettings.GetString("Local.ClientSecret", true);
            var credentials = new AuthenticationCredentials { ClientId = clientId, ClientSecret = clientSecret };
            var tokenRefresher =
                AuthenticationManager.CreateTokenRefresher(FulcrumApplication.Setup.Tenant, LocalAuthenticationBaseUrl, credentials);
            return tokenRefresher;
        }
        #endregion

        #region Configure

        /// <inheritdoc/>
        protected override void ConfigureAppMiddleware(IApplicationBuilder app, IHostingEnvironment env)
        {
            base.ConfigureAppMiddleware(app, env);
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
            FulcrumValidate.IsNotNull(_nexusLinkTokenRefresher, nameof(_nexusLinkTokenRefresher), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(LocalAuthenticationBaseUrl, nameof(LocalAuthenticationBaseUrl), errorLocation);
            FulcrumValidate.IsNotNull(_localTokenRefresher, nameof(_localTokenRefresher), errorLocation);
        }
    }
}
#endif