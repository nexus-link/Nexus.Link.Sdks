using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public class ServiceMetasClient : BaseClient, IServiceMetasClient
    {
        public ServiceMetasClient(string baseUri) : base(baseUri)
        {
        }

        public async Task<HealthResponse> ServiceHealthAsync(CancellationToken cancellationToken = default)
        {
            const string relativeUrl = "ServicesMeta/ServiceHealth";
            var result = await RestClient.GetAsync<HealthResponse>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }
    }
}
