using Microsoft.Rest;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Base
{
    public abstract class BaseClient
    {
        private readonly HttpSender _httpSender;
        protected readonly IRestClient RestClient;

        public IHttpClient HttpClient
        {
            get => _httpSender.HttpClient;
            set => _httpSender.HttpClient = value;
        }

        public string Organization { get; protected set; }

        public string Environment { get; protected set; }

        private static string GetUriStart(string baseUri)
        {
            return $"{baseUri}/api/v1";
        }

        protected BaseClient(string baseUri)
        {
            _httpSender = new HttpSender(GetUriStart(baseUri), new BasicAuthenticationCredentials());
            RestClient = new RestClient(_httpSender); // Just any auth, can't be null
        }

        protected BaseClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
        {
            Organization = tenant.Organization;
            Environment = tenant.Environment;
            var uri = $"{GetUriStart(baseUri)}/{tenant.Organization}/{tenant.Environment}";
            _httpSender = new HttpSender(uri, authenticationCredentials);
            RestClient = new RestClient(_httpSender);
        }

    }
}
