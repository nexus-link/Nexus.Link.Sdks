using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public class ServiceMetasClient : BaseClient, IServiceMetasClient
    {
        public ServiceMetasClient(string baseUri) : base(baseUri)
        {
        }

        public async Task<HealthResponse> ServiceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireValidated(tenant, nameof(tenant));
            var relativeUrl = $"ServiceMetas/{tenant.Organization}/{tenant.Environment}/ServiceHealth";
            var result = await RestClient.GetAsync<HealthResponse>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }
    }
}
