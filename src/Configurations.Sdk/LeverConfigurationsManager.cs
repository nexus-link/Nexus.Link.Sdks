using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.Pipe.Outbound;

namespace Nexus.Link.Configurations.Sdk
{
    /// <summary>
    /// Knows how to get configurations for tenants. Has a cache, so only the first get for each tenant costs an HTTP call. The following are taken from the cache.
    /// </summary>
    public class LeverConfigurationsManager : ILeverConfigurationsManager
    {
        private static readonly string Namespace = typeof(LeverConfigurationsManager).Namespace;
        private static ConfigurationCache _cache = new ConfigurationCache();
        private static readonly HttpClient HttpClient = HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlers());

        private readonly Uri _configurationsServiceUri;
        private readonly string _serviceName;
        private readonly ServiceClientCredentials _serviceClientCredentials;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configurationsServiceUrl">The URL to the configuration service</param>
        /// <param name="serviceName">The name for the current service.</param>
        /// <param name="serviceClientCredentials">AuthenticationCredentials for the current service to the configuration service.</param>
        public LeverConfigurationsManager(string configurationsServiceUrl, string serviceName, ServiceClientCredentials serviceClientCredentials)
        {
            InternalContract.RequireNotNull(configurationsServiceUrl, nameof(configurationsServiceUrl));
            InternalContract.RequireNotNull(serviceName, nameof(serviceName));
            InternalContract.RequireNotNull(serviceClientCredentials, nameof(serviceClientCredentials));

            if (!configurationsServiceUrl.EndsWith("/")) configurationsServiceUrl += "/";
            _configurationsServiceUri = new Uri(configurationsServiceUrl);
            _serviceName = serviceName;
            _serviceClientCredentials = serviceClientCredentials;
        }

        /// <inheritdoc />
        public async Task<ILeverConfiguration> GetConfigurationForAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));

            var configuration = _cache.Get(tenant, _serviceName);
            if (configuration != null) return configuration;

            var serviceUri = new Uri(_configurationsServiceUri, $"api/v1/{tenant.Organization}/{tenant.Environment}/Configurations/{_serviceName}");
            var jObject = await GetConfigurationAsync(serviceUri, cancellationToken);
            configuration = new LeverConfiguration(tenant, _serviceName, jObject);
            _cache.Add(tenant, _serviceName, configuration);
            return configuration;
        }

        private async Task<JObject> GetConfigurationAsync(Uri serviceUri, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(serviceUri, nameof(serviceUri));

            var request = new HttpRequestMessage(HttpMethod.Get, serviceUri);
            await _serviceClientCredentials.ProcessHttpRequestAsync(request, default);
            var response = await HttpClient.SendAsync(request, cancellationToken);
            FulcrumAssert.IsNotNull(response, $"{Namespace}: 332DE55E-8A40-482A-BA9D-0223C077A096", $"Expected a non-null response from GET {serviceUri}");
            FulcrumAssert.IsTrue(response.IsSuccessStatusCode, $"{Namespace}: BB3F6675-E5A6-4C96-924D-53FB18B636A4", $"Response status code was {response.StatusCode}. Expected all non-success responses to be thrown as Fulcrum Exceptions.");
            var configurationString = response.Content == null ? null : await response.Content.ReadAsStringAsync();
            // ReSharper disable once PossibleNullReferenceException
            FulcrumAssert.IsNotNullOrWhiteSpace(configurationString, $"{Namespace}: 8E18033D-F62C-41A6-BE0F-62F33CA21798", $"Empty response from {serviceUri}.");
            var configuration = JObject.Parse(configurationString);
            return configuration;
        }

        /// <inheritdoc />
        public void ClearCache()
        {
            _cache = new ConfigurationCache();
        }
    }
}
