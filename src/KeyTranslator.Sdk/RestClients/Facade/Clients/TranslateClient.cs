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

        public async Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceInstancePath, string targetContextName)
        {
            var relativeUrl = $"Translate/{WebUtility.UrlEncode(sourceInstancePath)}/Context/{WebUtility.UrlEncode(targetContextName)}/Lock";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl);
            return result;
        }

        public async Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceInstancePath, string targetClientName)
        {
            string relativeUrl = $"Translate/{WebUtility.UrlEncode(sourceInstancePath)}/Client/{WebUtility.UrlEncode(targetClientName)}/Lock";
            var result = await RestClient.GetAsync<ValueOrLockId>(relativeUrl);
            return result;
        }

        public async Task ReleaseLockAsync(string instancePath, string lockId)
        {
            string relativeUrl = $"Translate/{WebUtility.UrlEncode(instancePath)}/ReleaseLock";
            await RestClient.PostNoResponseContentAsync(relativeUrl, lockId);
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
