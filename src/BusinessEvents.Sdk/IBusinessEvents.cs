using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.BusinessEvents.Sdk
{
    public interface IBusinessEvents : IResourceHealth
    {
        /// <summary>
        /// Publish a business event based on version of event and publisher client.
        /// </summary>
        /// <param name="entityName">The name of the entity</param>
        /// <param name="eventName">The name of the event</param>
        /// <param name="majorVersion">The major version</param>
        /// <param name="minorVersion">The minor version</param>
        /// <param name="clientName">The publisher client</param>
        /// <param name="eventBody">The event body to be sent.</param>
        /// <param name="cancellationToken"></param>
        Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody, CancellationToken cancellationToken = default);

        /// <summary>
        /// Test publish an event to see if it meets the requirements (types and translations)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="event"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="clientName"></param>
        /// <param name="payload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PublicationTestResult> TestBenchPublish(string entity, string @event, int major, int minor, string clientName, JToken payload, CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers subscriptions for a client. The provided list of subscriptions will replace the previous subscriptions.
        ///
        /// Transactional, so that either all subscriptions are registered, or no change is done.
        ///
        /// Marks Client.DynamicalRegistration =  true.
        /// </summary>
        Task RegisterSubscriptions(string clientName, List<ClientSubscription> clientSubscriptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Business Events health for a tenant 
        /// </summary>
        Task<Health> TenantHealthAsync(Tenant tenant, CancellationToken cancellationToken = default);
    }
}