using System.Net.Http;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.AppSupport
{
    /// <inheritdoc cref="ILoggingService" />
    public class ConfigurationRestService : CrudRestClient<JToken, string>, IConfigurationService
    {
        
        /// <inheritdoc cref="IConfigurationService"/>
        public ConfigurationRestService(string baseUrl, HttpClient httpClient, ServiceClientCredentials credentials)
            :base(baseUrl, httpClient, credentials)
        {
        }
    }
}
