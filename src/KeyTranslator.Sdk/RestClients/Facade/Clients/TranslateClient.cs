using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients
{
    public class TranslateClient : BaseClient, ITranslateClient
    {

        public TranslateClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
        }

        [Obsolete("Consider using TranslateToContextOrLock2Async, since this older version does not support colons and slashes in the instance path")]
        public async Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceInstancePath, string targetContextName)
        {
            var relativeUrl = $"Translate/Lock?sourceInstancePath={WebUtility.UrlEncode(sourceInstancePath)}&targetContextName={WebUtility.UrlEncode(targetContextName)}";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl);
            return result;
        }

        public async Task<ValueOrLockId> TranslateToContextOrLock2Async(string sourceInstancePath, string targetContextName)
        {
            var relativeUrl = $"Translate/Lock?sourceInstancePath={WebUtility.UrlEncode(sourceInstancePath)}&targetContextName={WebUtility.UrlEncode(targetContextName)}";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl);
            return result;
        }

        [Obsolete("Consider using TranslateToClientOrLock2Async, since this older version does not support colons and slashes in the instance path")]
        public async Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceInstancePath, string targetClientName)
        {
            var relativeUrl = $"Translate/Lock?sourceInstancePath={WebUtility.UrlEncode(sourceInstancePath)}&targetClientName={WebUtility.UrlEncode(targetClientName)}";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl);
            return result;
        }

        public async Task<ValueOrLockId> TranslateToClientOrLock2Async(string sourceInstancePath, string targetClientName)
        {
            var relativeUrl = $"Translate/Lock?sourceInstancePath={WebUtility.UrlEncode(sourceInstancePath)}&targetClientName={WebUtility.UrlEncode(targetClientName)}";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl);
            return result;
        }

        [Obsolete("Consider using ReleaseLock2Async, since this older version does not support colons and slashes in the instance path")]
        public async Task ReleaseLockAsync(string instancePath, string lockId)
        {
            string relativeUrl = $"Translate/{WebUtility.UrlEncode(instancePath)}/ReleaseLock";
            await RestClient.PostNoResponseContentAsync(relativeUrl, lockId);
        }

        public async Task ReleaseLock2Async(string instancePath, string lockId)
        {
            const string relativeUrl = "Translate/ReleaseLock";
            await RestClient.PostNoResponseContentAsync(relativeUrl, new
            {
                InstancePath = instancePath,
                LockId = lockId
            });
        }

        public async Task<TranslateResponse> TranslateAsync(TranslateRequest translateRequest)
        {
            const string relativeUrl = "Translate";
            var result = await RestClient.PostAsync<TranslateResponse, TranslateRequest>(relativeUrl, translateRequest);
            return result;
        }

        public async Task<IEnumerable<TranslateResponse>> TranslateBatchAsync(IEnumerable<TranslateRequest> translateRequests)
        {

            const string relativeUrl = "Translate/Batch";
            var result = await RestClient.PostAsync<IEnumerable<TranslateResponse>, IEnumerable<TranslateRequest>>(relativeUrl, translateRequests);
            return result;
        }
    }
}
