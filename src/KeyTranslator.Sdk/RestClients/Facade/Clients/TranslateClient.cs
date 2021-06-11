using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.ValueTranslator;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public class TranslateClient : BaseClient, ITranslateClient
    {

        public TranslateClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
        }

        [Obsolete("Consider using TranslateToContextOrLock2Async, since this older version does not support colons and slashes in the instance path")]
        public async Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceInstancePath, string targetContextName,
            CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"Translate/Lock?sourceInstancePath={WebUtility.UrlEncode(sourceInstancePath)}&targetContextName={WebUtility.UrlEncode(targetContextName)}";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<ValueOrLockId> TranslateToContextOrLock2Async(string sourceInstancePath, string targetContextName,
            CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"Translate/Lock?sourceInstancePath={WebUtility.UrlEncode(sourceInstancePath)}&targetContextName={WebUtility.UrlEncode(targetContextName)}";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        [Obsolete("Consider using TranslateToClientOrLock2Async, since this older version does not support colons and slashes in the instance path")]
        public async Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceInstancePath, string targetClientName,
            CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"Translate/Lock?sourceInstancePath={WebUtility.UrlEncode(sourceInstancePath)}&targetClientName={WebUtility.UrlEncode(targetClientName)}";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<ValueOrLockId> TranslateToClientOrLock2Async(string sourceInstancePath, string targetClientName,
            CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"Translate/Lock?sourceInstancePath={WebUtility.UrlEncode(sourceInstancePath)}&targetClientName={WebUtility.UrlEncode(targetClientName)}";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        [Obsolete("Consider using ReleaseLock2Async, since this older version does not support colons and slashes in the instance path")]
        public async Task ReleaseLockAsync(string instancePath, string lockId,
            CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"Translate/{WebUtility.UrlEncode(instancePath)}/ReleaseLock";
            await RestClient.PostNoResponseContentAsync(relativeUrl, lockId, null, cancellationToken);
        }

        public async Task ReleaseLock2Async(string instancePath, string lockId,
            CancellationToken cancellationToken = default)
        {
            const string relativeUrl = "Translate/ReleaseLock";
            await RestClient.PostNoResponseContentAsync(relativeUrl, new ReleaseLockRequest
            {
                InstancePath = instancePath,
                LockId = lockId
            }, cancellationToken: cancellationToken);
        }

        public async Task<TranslateResponse> TranslateAsync(TranslateRequest translateRequest,
            CancellationToken cancellationToken = default)
        {
            const string relativeUrl = "Translate";
            var result = await RestClient.PostAsync<TranslateResponse, TranslateRequest>(relativeUrl, translateRequest, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<IEnumerable<TranslateResponse>> TranslateBatchAsync(IEnumerable<TranslateRequest> translateRequests,
            CancellationToken cancellationToken = default)
        {

            const string relativeUrl = "Translate/Batch";
            var result = await RestClient.PostAsync<IEnumerable<TranslateResponse>, IEnumerable<TranslateRequest>>(relativeUrl, translateRequests, cancellationToken: cancellationToken);
            return result;
        }
    }
}
