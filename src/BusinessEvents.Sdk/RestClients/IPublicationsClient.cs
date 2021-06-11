using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal interface IPublicationsClient
    { 
        /// <summary>
        /// Publish a business event.
        /// </summary>
        /// <param name="publicationId">The id of the publication.</param>
        /// <param name="content">The content of the event.</param>
        Task PublishAsync(Guid publicationId, JToken content, CancellationToken cancellationToken = default);

        [Obsolete]
        Task PublishAsync(Guid publicationId, JToken content, string correlationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send an event directly to a subscriber endpoint
        /// </summary>
        /// <param name="entityName">The name of the entity</param>
        /// <param name="eventName">The name of the event</param>
        /// <param name="majorVersion">The major version</param>
        /// <param name="minorVersion">The minor version</param>
        /// <param name="clientName">The publisher client</param>
        /// <param name="eventBody">The event body to be sent.</param>
        /// <param name="cancellationToken"></param>
        Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody, CancellationToken cancellationToken = default);

        [Obsolete]
        Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody, string correlationId, CancellationToken cancellationToken = default);

        Task PublishWithClientNameAsync(Guid publicationId, JToken content, string clientName, CancellationToken cancellationToken = default);
    }
}
