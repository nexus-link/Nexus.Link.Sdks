using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public interface IInstanceClient
    {

        Task<Instance> GetOneAsync(Guid instanceId, CancellationToken cancellationToken = default);

        Task<Instance> UpdateAsync(Guid instanceId, Instance instance, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid instanceId, CancellationToken cancellationToken = default);
    }
}
