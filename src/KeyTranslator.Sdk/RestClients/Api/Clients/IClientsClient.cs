using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public interface IClientsClient
    {

        Task<IEnumerable<Client>> GetAllAsync();

        Task<Client> GetOneAsync(string clientId);

        Task<Client> UpdateAsync(string clientId, Client client);

        Task<Client> CreateAsync(string technicalName);

        Task DeleteAsync(string clientId);

        Task<DefaultContext> CreateDefaultContextAsync(string clientId, string conceptId, string contextId);

        Task DeleteDefaultContextAsync(string clientId, string conceptId);
    }
}
