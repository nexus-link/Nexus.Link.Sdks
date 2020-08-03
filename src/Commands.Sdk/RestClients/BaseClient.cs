using System;
using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Commands.Sdk.RestClients
{
    public abstract class BaseClient
    {
        protected readonly IRestClient RestClient;

        private static string GetUriStart(Tenant tenant, string baseUri)
        {
            if (string.IsNullOrWhiteSpace(baseUri)) throw new ArgumentException($"{nameof(baseUri)} can't be null or empty");
            var isWelFormedUri = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWelFormedUri) throw new ArgumentException($"{nameof(baseUri)} must be a well formed uri");

            return $"{baseUri}/api/v1/Organizations/{tenant.Organization}/Environments/{tenant.Environment}/Commands";
        }

        protected BaseClient(Tenant tenant, string baseUri, ServiceClientCredentials authenticationCredentials)
        {
            var httpSender = new HttpSender(GetUriStart(tenant, baseUri), authenticationCredentials)
            {
                HttpClient = new HttpClientWrapper(HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlersWithoutLogging()))
            };
            RestClient = new RestClient(httpSender);
        }
    }
}
