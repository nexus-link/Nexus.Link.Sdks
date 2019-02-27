using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public class AssociationsClient : BaseClient, IAssociationsClient
    {

        public AssociationsClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
        }

        public async Task CreateAsync(Association association)
        {
            const string relativeUrl = "/Associations";
            await RestClient.PostNoResponseContentAsync(relativeUrl, association);
        }
    }
}
