using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.Platform.ValueTranslator;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public interface ITranslateClient
    {
        string Organization { get; }
        string Environment { get; }
        IHttpClient HttpClient { get; set; }

        Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceInstancePath, string targetClientName,
            CancellationToken cancellationToken = default);
        Task<ValueOrLockId> TranslateToClientOrLock2Async(string sourceInstancePath, string targetClientName,
            CancellationToken cancellationToken = default);

        Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceInstancePath, string targetContextName,
            CancellationToken cancellationToken = default);
        Task<ValueOrLockId> TranslateToContextOrLock2Async(string sourceInstancePath, string targetContextName,
            CancellationToken cancellationToken = default);

        Task ReleaseLockAsync(string instancePath, string lockId,
            CancellationToken cancellationToken = default);
        Task ReleaseLock2Async(string instancePath, string lockId,
            CancellationToken cancellationToken = default);

        Task<TranslateResponse> TranslateAsync(TranslateRequest translateRequest,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TranslateResponse>> TranslateBatchAsync(IEnumerable<TranslateRequest> translateRequests,
            CancellationToken cancellationToken = default);
    }
}
