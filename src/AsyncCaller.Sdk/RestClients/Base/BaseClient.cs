using Microsoft.Rest;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncCaller.Sdk.RestClients.Base
{
    public abstract class BaseClient
    {
        protected readonly IRestClient RestClient;

        private static string GetUriStart(string baseUrl)
        {
            return $"{baseUrl}/api/v1";
        }

        protected BaseClient(string baseUrl)
        {
            RestClient = new RestClient(new HttpSender(GetUriStart(baseUrl)));
        }

        protected BaseClient(string baseUrl, Tenant tenant, ServiceClientCredentials credentials)
        {
            var uri = $"{GetUriStart(baseUrl)}/{tenant.Organization}/{tenant.Environment}";
            RestClient = new RestClient(new HttpSender(uri, credentials));
        }

    }
}
