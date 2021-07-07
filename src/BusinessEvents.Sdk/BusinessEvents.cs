using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.BusinessEvents.Sdk.RestClients;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.BusinessEvents.Sdk
{
    /// <summary>
    /// SDK for publishing business events.
    /// </summary>
    public class BusinessEvents : IBusinessEvents
    {
        private readonly IPublicationsClient _publicationsClient;
        private readonly ISubscriptionsClient _subscriptionsClient;
        private readonly IServiceMetasClient _serviceMetasClient;
        private readonly ITestBenchClient _testBenchClient;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceUrl">The URL for the BusinessEvents service.</param>
        /// <param name="tenant">The tenant we want to publish in.</param>
        /// <param name="authenticationCredentials">How we can get credentials when calling the service.</param>
        public BusinessEvents(string serviceUrl, Tenant tenant, ServiceClientCredentials authenticationCredentials)
        {
            InternalContract.RequireNotNullOrWhiteSpace(serviceUrl, nameof(serviceUrl));
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireValidated(tenant, nameof(tenant));
            InternalContract.RequireNotNull(authenticationCredentials, nameof(authenticationCredentials));

            _publicationsClient = new PublicationsClient(serviceUrl, tenant, authenticationCredentials);
            _subscriptionsClient = new SubscriptionsClient(serviceUrl, tenant, authenticationCredentials);
            _serviceMetasClient = new ServiceMetasClient(serviceUrl);
            _testBenchClient = new TestBenchClient(serviceUrl, tenant, authenticationCredentials);
        }

        /// <summary>
        /// Constructor for using the <see cref="MockService"/>.
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="uris"></param>
        public BusinessEvents(ServiceClientCredentials credentials, IEnumerable<string> uris)
        {
            InternalContract.Require(FulcrumApplication.IsInDevelopment,
                "This constructor is for mock purposes and is only valid in development.");
            InternalContract.RequireNotNull(credentials, nameof(credentials));
            InternalContract.RequireNotNull(uris, nameof(uris));
            _publicationsClient = new MockService(credentials, uris);
        }

        /// <summary>
        /// Publish a business event based on the publication id.
        /// </summary>
        /// <param name="publicationId">The id of the publication.</param>
        /// <param name="content">The content of the event.</param>
        /// <param name="correlationId">Optional Correlation id</param>
        /// <param name="cancellationToken"></param>
        [Obsolete("Use version without correlation id", false)]
        public async Task PublishAsync(Guid publicationId, JToken content, string correlationId, CancellationToken cancellationToken = default)
        {
            await PublishAsync(publicationId, content, cancellationToken);
        }

        /// <summary>
        /// Publish a business event based on the publication id.
        /// </summary>
        /// <param name="publicationId">The id of the publication.</param>
        /// <param name="content">The content of the event.</param>
        /// <param name="cancellationToken"></param>
        public async Task PublishAsync(Guid publicationId, JToken content, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(content, nameof(content));
            await _publicationsClient.PublishAsync(publicationId, content, cancellationToken);
        }

        /// <summary>
        /// Publish a business event based on the publication id, with the publising client name.
        /// </summary>
        /// <param name="publicationId">The id of the publication.</param>
        /// <param name="content">The content of the event.</param>
        /// <param name="clientName">The technical name of the publishing client</param>
        /// <param name="cancellationToken"></param>
        public async Task PublishWithClientNameAsync(Guid publicationId, JToken content, string clientName, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(content, nameof(content));
            InternalContract.RequireNotNull(clientName, nameof(content));
            await _publicationsClient.PublishWithClientNameAsync(publicationId, content, clientName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(eventBody, nameof(eventBody));

            await _publicationsClient.PublishAsync(entityName, eventName, majorVersion, minorVersion, clientName, eventBody, cancellationToken);
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        [Obsolete("Use version without correlation id", true)]
        public async Task PublishAsync(string entityName, string eventName, int majorVersion, int minorVersion, string clientName, JToken eventBody, string correlationId, CancellationToken cancellationToken = default)
        {
            await PublishAsync(entityName, eventName, majorVersion, minorVersion, clientName, eventBody, cancellationToken);
        }

        /// <summary>
        /// Get the health for the BusinessEvents service.
        /// </summary>
        /// <returns>A description of the health for the BusinessEvents service.</returns>
        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            var result = await _serviceMetasClient.GetResourceHealthAsync(tenant, cancellationToken);
            return result;
        }

        /// <inheritdoc />
        public async Task<PublicationTestResult> TestBenchPublish(string entity, string @event, int major, int minor, string clientName, JToken payload, CancellationToken cancellationToken = default)
        {
            return await _testBenchClient.PublishAsync(entity, @event, major, minor, clientName, payload, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RegisterSubscriptions(string clientName, List<ClientSubscription> clientSubscriptions, CancellationToken cancellationToken = default)
        {
            await _subscriptionsClient.RegisterSubscriptions(clientName, clientSubscriptions, cancellationToken);
        }
    }
}
