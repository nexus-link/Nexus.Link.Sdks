using System;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal class PublicationsClient : BaseClient, IPublicationsClient
    {
        public PublicationsClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
            var isWellFormedUriString = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWellFormedUriString) throw new ArgumentException($"{nameof(baseUri)} must be a well formed uri");
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            if (authenticationCredentials == null) throw new ArgumentNullException(nameof(tenant));

        }

        public async Task PublishAsync(Guid publicationId, JToken content)
        {
            await PublishWithClientNameAsync(publicationId, content, null);
        }

        [Obsolete]
        public async Task PublishAsync(Guid publicationId, JToken content, string correlationId)
        {
            await PublishAsync(publicationId, content);
        }

        public async Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody)
        {
            InternalContract.RequireNotNullOrWhiteSpace(entityName, nameof(entityName));
            InternalContract.RequireNotNullOrWhiteSpace(eventName, nameof(eventName));
            InternalContract.RequireGreaterThanOrEqualTo(0, majorVersion, nameof(majorVersion));
            InternalContract.RequireGreaterThanOrEqualTo(0, minorVersion, nameof(minorVersion));
            InternalContract.RequireNotNull(eventBody, nameof(eventBody));

            var relativeUrl = $"Publications/{entityName}/{eventName}/{majorVersion}/{minorVersion}?clientName={clientName}";
            await RestClient.PostNoResponseContentAsync(relativeUrl, eventBody);
        }

        [Obsolete]
        public async Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody, string correlationId)
        {
            await PublishAsync(entityName, eventName, majorVersion, minorVersion, clientName, eventBody);
        }

        public async Task PublishWithClientNameAsync(Guid publicationId, JToken content, string clientName)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var relativeUrl = $"Publications/{publicationId}";
            if (!string.IsNullOrWhiteSpace(clientName)) relativeUrl += $"/AsClient/{clientName}";
            await RestClient.PostNoResponseContentAsync(relativeUrl, content);
        }
    }
}
