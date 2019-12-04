using System;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal abstract class BaseClient
    {
        protected readonly IRestClient RestClient;

        private static string GetUriStart(string baseUri)
        {
            if (string.IsNullOrWhiteSpace(baseUri)) throw new ArgumentException($"{nameof(baseUri)} can't be null or empty");
            var isWelFormedUri = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if(!isWelFormedUri) throw new ArgumentException($"{nameof(baseUri)} must be a welformed uri");

            return $"{baseUri}/api/v1";
        }

        protected BaseClient(string baseUri)
        {
            if (string.IsNullOrWhiteSpace(baseUri)) throw new ArgumentException($"{nameof(baseUri)} can't be null or empty");
            var uri = GetUriStart(baseUri);
            RestClient = new RestClient(new HttpSender(uri, new BasicAuthenticationCredentials())); // Just any credentials
        }


        protected BaseClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
        {
            var uri = $"{GetUriStart(baseUri)}/{tenant.Organization}/{tenant.Environment}";
            RestClient = new RestClient(new HttpSender(uri, authenticationCredentials));
        }

    }
}
