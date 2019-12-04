#pragma warning disable 1591

using System;
using System.Net.Http;
using Microsoft.Rest;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Logger.Sdk.RestClients
{
    public abstract class BaseClient
    {
        protected readonly IRestClient RestClient;

        private static string GetUriStart(string baseUri)
        {
            if (string.IsNullOrWhiteSpace(baseUri)) throw new ArgumentException($"{nameof(baseUri)} can't be null or empty");
            var isWelFormedUri = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWelFormedUri) throw new ArgumentException($"{nameof(baseUri)} must be a well formed uri");

            return $"{baseUri}/api/v1";
        }

        protected BaseClient(string baseUri, ServiceClientCredentials authenticationCredentials)
        {
            var httpSender = new HttpSender(GetUriStart(baseUri), authenticationCredentials)
            {
                HttpClient = new HttpClientWrapper(HttpClientFactory.Create(OutboundPipeFactory.CreateDelegatingHandlersWithoutLogging()))
            };
            RestClient = new RestClient(httpSender);
        }
    }
}
