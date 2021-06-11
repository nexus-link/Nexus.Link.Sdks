using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    /// <inheritdoc cref="IConceptsClient" />
    public class ConceptsClient : BaseClient, IConceptsClient
    {
        /// <inheritdoc />
        public ConceptsClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDictionary<string, string>>> GetAllInstancesAsync(string conceptName,
            CancellationToken cancellationToken = default)
        {
            var result = await RestClient.GetAsync<IEnumerable<IDictionary<string, string>>>($"Concepts/{conceptName}/Instances", null, cancellationToken);
            return result;
        }
    }
}
