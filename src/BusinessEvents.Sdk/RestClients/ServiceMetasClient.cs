using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal class ServiceMetasClient : BaseClient, IServiceMetasClient
    {
        public ServiceMetasClient(string baseUri) : base(baseUri)
        {
            var isWelFormedUri = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWelFormedUri) throw new ArgumentException($"{nameof(baseUri)} must be a welformed uri");
        }

        [Obsolete("Please use TenantHealthAsync instead")]
        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"{WebUtility.UrlEncode(tenant.Organization)}/{WebUtility.UrlEncode(tenant.Environment)}/ServiceMetas/ServiceHealth";
            var result = await RestClient.GetAsync<HealthResponse>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Health> TenantHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"{WebUtility.UrlEncode(tenant.Organization)}/{WebUtility.UrlEncode(tenant.Environment)}/ServiceMetas/Health";
            var result = await RestClient.GetAsync<Health>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }
    }
}
