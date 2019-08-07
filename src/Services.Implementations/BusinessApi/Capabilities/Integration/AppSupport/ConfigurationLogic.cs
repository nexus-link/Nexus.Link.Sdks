using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.AppSupport
{
    /// <inheritdoc cref="IConfigurationService" />
    public class ConfigurationLogic : CrudRestClient<JToken, string>, IConfigurationService
    {
        /// <inheritdoc />
        public ConfigurationLogic(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials)
            : base($"{baseUri}/Configurations", httpClient, credentials)
        {
        }

        /// <inheritdoc />
        public override async Task<JToken> ReadAsync(string id, CancellationToken token = new CancellationToken())
        {
            try
            {
                return await base.ReadAsync(id, token);
            }
            catch (FulcrumNotFoundException)
            {
                return new JObject();
            }
        }
    }
}
