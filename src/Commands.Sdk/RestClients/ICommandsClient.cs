using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nexus.Link.Commands.Sdk.RestClients
{
    public interface ICommandsClient
    {
        Task<NexusCommand> CreateAsync(string service, string command, string originator);
        Task<IEnumerable<NexusCommand>> ReadAsync(string service, string instanceId);
    }
}
