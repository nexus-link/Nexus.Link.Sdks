using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;
using Nexus.Link.Services.Contracts.Events;
using Nexus.Link.Services.Implementations.Adapter.Events;
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.AppSupport;
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.Authentication;
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.ValueTranslation;
using Nexus.Link.Services.Implementations.BusinessApi.Startup.Configuration;
#if NETCOREAPP

namespace Nexus.Link.Services.Implementations.BusinessApi.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public abstract class NexusBusinessApiStartup : NexusCommonStartup
    {
        private static readonly object ClassLock = new object();

        /// <summary>
        /// Access to all relevant configuration
        /// </summary>
        protected BusinessApiConfiguration BusinessApiConfiguration { get; }

        #region Configure Services
        /// <inheritdoc />
        protected override void DependencyInjectServices(IServiceCollection services)
        {
            base.DependencyInjectServices(services);
            //
            // Nexus services
            //

            // Authentication
            services.AddScoped<IAuthenticationCapability>(provider =>
                ValidateDependencyInjection(provider,
                    p => new AuthenticationCapability(BusinessApiConfiguration.AuthenticationLocal.Endpoint, GetLocalCredentials())));

            // Business Events
            services.AddScoped<IBusinessEventsCapability>(provider =>
                ValidateDependencyInjection(provider, p =>
                    new BusinessEventsCapability(BusinessApiConfiguration.NexusCapabilityEndpoints.BusinessEvents, GetNexusCredentials())));

            // Value Translation
            var valueTranslationCapability = new ValueTranslationCapability(
                BusinessApiConfiguration.NexusCapabilityEndpoints.ValueTranslation, GetNexusCredentials());
            services.AddSingleton<IValueTranslationCapability>(provider => valueTranslationCapability);
            services.AddSingleton(provider => valueTranslationCapability.TranslatorService);

            // App support
            services.AddScoped<IAppSupportCapability>(provider =>
                ValidateDependencyInjection(provider, p =>
                {
                    var httpSender = new HttpSender(BusinessApiConfiguration.NexusCapabilityEndpoints.AppSupport, GetNexusCredentials());
                    return new AppSupportCapability(null, httpSender);
                }));
        }

        /// <summary>
        /// A token generator for authenticating between adapters and the business API.
        /// </summary>
        public ITokenRefresherWithServiceClient LocalTokenRefresher {
            get
            {
                lock(ClassLock)
                {
                    if (_localTokenRefresher != null) return _localTokenRefresher;
                    _localTokenRefresher =
                        AuthenticationManager.CreateTokenRefresher(
                            FulcrumApplication.Setup.Tenant,
                            BusinessApiConfiguration.AuthenticationLocal.Endpoint, 
                            BusinessApiConfiguration.AuthenticationLocal.Credentials);
                    return _localTokenRefresher;
                }
            }
        }

        private ITokenRefresherWithServiceClient _localTokenRefresher;

        /// <summary>
        /// A token generator for authenticating your app vs. Nexus Link.
        /// </summary>
        /// <remarks>Only useful for the Business API, not for Nexus Adapters.</remarks>
        public ITokenRefresherWithServiceClient NexusLinkTokenRefresher {
            get
            {
                lock(ClassLock)
                {
                    if (_nexusLinkTokenRefresher != null) return _nexusLinkTokenRefresher;
                    _nexusLinkTokenRefresher =
                        NexusAuthenticationManager.CreateTokenRefresher(
                            FulcrumApplication.Setup.Tenant,
                            BusinessApiConfiguration.AuthenticationNexus.Endpoint, 
                            BusinessApiConfiguration.AuthenticationNexus.Credentials);
                    return _nexusLinkTokenRefresher;
                }
            }
        }

        private ITokenRefresherWithServiceClient _nexusLinkTokenRefresher;

        /// <summary>
        /// Credentials for calling Nexus Link services
        /// </summary>
        protected ServiceClientCredentials GetNexusCredentials() => NexusLinkTokenRefresher.GetServiceClient();

        /// <summary>
        /// Credentials for calling adapters
        /// </summary>
        protected ServiceClientCredentials GetLocalCredentials()=> LocalTokenRefresher.GetServiceClient();

        /// <inheritdoc/>
        protected NexusBusinessApiStartup(IConfiguration configuration) : base(configuration, true)
        {
            BusinessApiConfiguration = new BusinessApiConfiguration(configuration);
        }
        #endregion

        #region Configure

        /// <inheritdoc/>
        protected override void ConfigureAppMiddleware(IApplicationBuilder app, IHostingEnvironment env)
        {
            base.ConfigureAppMiddleware(app, env);
            // Verify tokens with our public key
            var rsaPublicKey = AuthenticationManager
                .GetPublicRsaKeyAsync(FulcrumApplication.Setup.Tenant, BusinessApiConfiguration.AuthenticationLocal.Endpoint).Result;
            app.UseNexusTokenValidationHandler(rsaPublicKey);
        }
        #endregion

        /// <summary>
        /// Support method for getting a value from configuration and throwing an exception if it was missing.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string GetMandatoryString(IConfiguration configuration, string key)
        {
            var value = configuration.GetValue<string>(key);
            FulcrumAssert.IsNotNullOrWhiteSpace(value, CodeLocation.AsString(), $"Missing configuration {key}");
            return value;
        }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            base.Validate(errorLocation, propertyPath);
            FulcrumValidate.IsNotNull(NexusLinkTokenRefresher, nameof(NexusLinkTokenRefresher), errorLocation);
            FulcrumValidate.IsNotNull(LocalTokenRefresher, nameof(LocalTokenRefresher), errorLocation);
        }
    }
}
#endif