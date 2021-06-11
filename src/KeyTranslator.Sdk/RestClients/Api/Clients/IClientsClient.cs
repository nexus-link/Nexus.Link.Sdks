using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public interface IClientsClient
    {

        Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Client> GetOneAsync(string clientId, CancellationToken cancellationToken = default);

        Task<Client> UpdateAsync(string clientId, Client client, CancellationToken cancellationToken = default);

        Task<Client> CreateAsync(string technicalName, CancellationToken cancellationToken = default);

        Task DeleteAsync(string clientId, CancellationToken cancellationToken = default);

        Task<DefaultContext> CreateDefaultContextAsync(string clientId, string conceptId, string contextId, CancellationToken cancellationToken = default);

        Task DeleteDefaultContextAsync(string clientId, string conceptId, CancellationToken cancellationToken = default);
    }
}
