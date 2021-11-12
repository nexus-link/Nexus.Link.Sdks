using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json;
using Nexus.Link.KeyTranslator.Sdk.Models;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public class InstancesClient : BaseClient, IInstancesClient
    {
        public InstancesClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
        }

        public async Task<IDictionary<string, InstancesExistsResult>> InstancesExists(IEnumerable<string> conceptKeys, CancellationToken cancellationToken = default)
        {
            const string relativeUrl = "Instances/Exists";
            return await RestClient.PostAsync<IDictionary<string, InstancesExistsResult>, IEnumerable<string>>(relativeUrl, conceptKeys, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(ConceptValue conceptValue, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(conceptValue, nameof(conceptValue));
            InternalContract.RequireValidated(conceptValue, nameof(conceptValue));

            var relativeUrl = $"Instances";
            await RestClient.SendRequestAsync(HttpMethod.Delete, relativeUrl, conceptValue, null, cancellationToken);
        }
    }
}
