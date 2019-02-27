using System;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public interface IInstanceClient
    {

        Task<Instance> GetOneAsync(Guid instanceId);

        Task<Instance> UpdateAsync(Guid instanceId, Instance instance);

        Task DeleteAsync(Guid instanceId);
    }
}
