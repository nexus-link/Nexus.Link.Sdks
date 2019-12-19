using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.DatasyncEngine.Sdk.RestClients
{
    public class ServiceMetasClient : BaseClient, IServiceMetasClient
    {
        public ServiceMetasClient(string baseUri) : base(baseUri)
        {
            var isWelFormedUri = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWelFormedUri) throw new ArgumentException($"{nameof(baseUri)} must be a welformed uri");
        }

        public async Task<HealthInfo> GetResourceHealth2Async(Tenant tenant)
        {
            var relativeUrl = $"{tenant.Organization}/{tenant.Environment}/ServiceMetas/Health";
            var result = await RestClient.GetAsync<HealthInfo>(relativeUrl);
            return result;
        }

        public async Task<HealthInfo> GetResourceHealth2Async()
        {
            const string relativeUrl = "ServiceMetas/Health";
            var result = await RestClient.GetAsync<HealthInfo>(relativeUrl);
            return result;
        }
    }
}
