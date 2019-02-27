using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public interface ITranslateClient
    {
        string Organization { get; }
        string Environment { get; }
        Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceInstancePath, string targetClientName);

        Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceInstancePath, string targetContextName);

        Task ReleaseLockAsync(string instancePath,  string lockId);

        Task<TranslateResponse> TranslateAsync(TranslateRequest translateRequest);

        Task<IEnumerable<TranslateResponse>> TranslateBatchAsync(IEnumerable<TranslateRequest> translateRequests);
    }
}
