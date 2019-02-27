using System;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal class TestBenchClient : BaseClient, ITestBenchClient
    {
        public TestBenchClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
            var isWelFormedUri = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWelFormedUri) throw new ArgumentException($"{nameof(baseUri)} must be a welformed uri");
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            if (authenticationCredentials == null) throw new ArgumentNullException(nameof(tenant));
        }

        public async Task<PublicationTestResult> Publish(string entity, string @event, int major, int minor, string clientName, JToken payload)
        {
            var relativeUrl = $"TestBench/{entity}/{@event}/{major}/{minor}/Publish?clientName={clientName}";
            return await RestClient.PostAsync<PublicationTestResult, JToken>(relativeUrl, payload);
        }
    }
}
