using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public interface ITranslateClient
    {
        string Organization { get; }
        string Environment { get; }
        IHttpClient HttpClient { get; set; }

        Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceInstancePath, string targetClientName,
            CancellationToken cancellationToken = default (CancellationToken));
        Task<ValueOrLockId> TranslateToClientOrLock2Async(string sourceInstancePath, string targetClientName,
            CancellationToken cancellationToken = default (CancellationToken));

        Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceInstancePath, string targetContextName,
            CancellationToken cancellationToken = default (CancellationToken));
        Task<ValueOrLockId> TranslateToContextOrLock2Async(string sourceInstancePath, string targetContextName,
            CancellationToken cancellationToken = default (CancellationToken));

        Task ReleaseLockAsync(string instancePath, string lockId,
            CancellationToken cancellationToken = default (CancellationToken));
        Task ReleaseLock2Async(string instancePath, string lockId,
            CancellationToken cancellationToken = default (CancellationToken));

        Task<TranslateResponse> TranslateAsync(TranslateRequest translateRequest,
            CancellationToken cancellationToken = default (CancellationToken));

        Task<IEnumerable<TranslateResponse>> TranslateBatchAsync(IEnumerable<TranslateRequest> translateRequests,
            CancellationToken cancellationToken = default (CancellationToken));
    }
}
