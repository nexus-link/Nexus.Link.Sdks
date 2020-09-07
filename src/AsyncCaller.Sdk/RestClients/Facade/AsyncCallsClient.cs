using System;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.AsyncCaller.Sdk.RestClients.Base;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using RawRequest = Nexus.Link.AsyncCaller.Sdk.Data.Models.RawRequest;

namespace Nexus.Link.AsyncCaller.Sdk.RestClients.Facade
{
    public class AsyncCallsClient : BaseClient, IAsyncCallsClient
    {

        public AsyncCallsClient(string baseUrl)
            : base(baseUrl)
        {
        }

        public AsyncCallsClient(string baseUrl, Tenant tenant, ServiceClientCredentials credentials)
            : base(baseUrl, tenant, credentials)
        {
        }

        [Obsolete("Please use PostRawAsync. Obsolete since 2020-09-10")]
        public async Task<string> PostAsync(Dispatcher.Models.Request request)
        {
            const string relativeUrl = "/AsyncCalls";
            return await RestClient.PostAsync<string, Dispatcher.Models.Request>(relativeUrl, request);
        }

        public async Task<string> PostRawAsync(RawRequest rawRequest)
        {
            const string relativeUrl = "/AsyncCalls/Raw";
            return await RestClient.PostAsync<string, RawRequest>(relativeUrl, rawRequest);
        }

        public async Task ClearAsync()
        {
            const string relativeUrl = "/AsyncCalls/Queues/Clear";
            await RestClient.PostNoResponseContentAsync(relativeUrl);
        }

        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant)
        {
            const string relativeUrl = "ServiceMetas/ServiceHealth";
            return await RestClient.GetAsync<HealthResponse>(relativeUrl);
        }
    }
}
