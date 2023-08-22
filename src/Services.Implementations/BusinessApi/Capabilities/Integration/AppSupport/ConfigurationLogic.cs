using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.AppSupport
{
    /// <inheritdoc cref="IConfigurationService" />
    public class ConfigurationLogic : CrudRestClient<JToken, string>, IConfigurationService
    {
        /// <inheritdoc />
        public ConfigurationLogic(IHttpSender httpSender)
            :base(httpSender.CreateHttpSender("Configurations"))
        {
        }

        /// <inheritdoc />
        public override async Task<JToken> ReadAsync(string id, CancellationToken token = default)
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
