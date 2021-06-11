using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public class ClientsClient : BaseClient, IClientsClient
    {
        public ClientsClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
        }

        public async Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string relativeUrl = "/Clients";
            var result = await RestClient.GetAsync<IEnumerable<Client>>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Client> GetOneAsync(string clientId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Clients/{clientId}";
            var result = await RestClient.GetAsync<Client>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Client> UpdateAsync(string clientId, Client client, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Clients/{clientId}";
            var result = await RestClient.PutAndReturnUpdatedObjectAsync(relativeUrl, client, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Client> CreateAsync(string technicalName, CancellationToken cancellationToken = default)
        {
            const string relativeUrl = "/Clients";
            var result = await RestClient.PostAsync<Client, string>(relativeUrl, technicalName, cancellationToken: cancellationToken);
            return result;
        }

        public async Task DeleteAsync(string clientId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Clients/{clientId}";
            await RestClient.DeleteAsync(relativeUrl, cancellationToken: cancellationToken);
        }

        public async Task<DefaultContext> CreateDefaultContextAsync(string clientId, string conceptId, string contextId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Clients/{clientId}/Concepts/{conceptId}/Contexts";
            var result = await RestClient.PostAsync<DefaultContext, string>(relativeUrl, contextId, cancellationToken: cancellationToken);
            return result;
        }

        public async Task DeleteDefaultContextAsync(string clientId, string conceptId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Clients/{clientId}/Concepts/{conceptId}/Contexts";
            await RestClient.DeleteAsync(relativeUrl, cancellationToken: cancellationToken);
        }
    }
}
