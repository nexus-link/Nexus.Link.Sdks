using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.BusinessEvents.Sdk.RestClients;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.BusinessEvents.Sdk
{
    /// <summary>
    /// A mock service for sending events directly to a fixed list of subscribers
    /// </summary>
    public class MockService : IPublicationsClient
    {
        private readonly List<IRestClient>_restClients;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="uris">The subscription urls</param>
        public MockService(ServiceClientCredentials credentials, IEnumerable<string> uris)
        {
            _restClients = new List<IRestClient>();
            foreach (var uri in uris)
            {
                _restClients.Add(new RestClient(uri, credentials));
            } 
        }

        /// <inheritdoc />
        public Task PublishAsync(Guid publicationId, JToken content)
        {
            throw new FulcrumNotImplementedException("Please use an other PublishAsync method.");
        }

        /// <inheritdoc />
        public Task PublishAsync(Guid publicationId, JToken content, string correlationId)
        {
            throw new FulcrumNotImplementedException("Please use an other PublishAsync method.");
        }

        /// <inheritdoc />
        public async Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody)
        {
            var relativeUrl = $"{entityName}/{eventName}/{majorVersion}/{minorVersion}?clientName={clientName}";
            foreach (var restClient in _restClients)
            {
                await restClient.PostNoResponseContentAsync(relativeUrl, eventBody);
            }
        }

        /// <inheritdoc />
        public Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody, string correlationId)
        {
            throw new FulcrumNotImplementedException("Please use an other PublishAsync method.");
        }

        /// <inheritdoc />
        public Task PublishWithClientNameAsync(Guid publicationId, JToken content, string clientName)
        {
            throw new FulcrumNotImplementedException("Please use an other PublishAsync method.");
        }
    }
}
