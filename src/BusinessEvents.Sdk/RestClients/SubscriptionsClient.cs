using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal class SubscriptionsClient : BaseClient, ISubscriptionsClient
    {
        public SubscriptionsClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials) : base(baseUri, tenant, authenticationCredentials)
        {
            var isWellFormedUriString = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWellFormedUriString) throw new ArgumentException($"{nameof(baseUri)} must be a well formed uri");
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            if (authenticationCredentials == null) throw new ArgumentNullException(nameof(tenant));
        }

        public async Task RegisterSubscriptions(string clientName, List<ClientSubscription> clientSubscriptions, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(clientName, nameof(clientName));
            InternalContract.RequireNotNull(clientSubscriptions, nameof(clientSubscriptions));

            var relativeUrl = $"Subscriptions/Clients/{WebUtility.UrlEncode(clientName)}";
            await RestClient.PostNoResponseContentAsync(relativeUrl, clientSubscriptions, cancellationToken: cancellationToken);
        }
    }
}
