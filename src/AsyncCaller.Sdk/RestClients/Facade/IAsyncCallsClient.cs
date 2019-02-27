using System.Threading.Tasks;
using Nexus.Link.AsyncCaller.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.Health.Model;

namespace Nexus.Link.AsyncCaller.Sdk.RestClients.Facade
{
    public interface IAsyncCallsClient : IResourceHealth
    {
        Task<string> PostAsync(Request request);
        Task<string> PostRawAsync(RawRequest rawRequest);
        Task ClearAsync();
    }
}
