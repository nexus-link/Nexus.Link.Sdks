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
    internal class ClientSubscriptionsClient : BaseClient, IClientSubscriptionsClient
    {
        public ClientSubscriptionsClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials) : base(baseUri, tenant, authenticationCredentials)
        {
            InternalContract.Require(Uri.IsWellFormedUriString(baseUri, UriKind.Absolute), $"{nameof(baseUri)} must be a well formed uri");
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNull(authenticationCredentials, nameof(authenticationCredentials));
        }

        public async Task RegisterSubscriptions(string clientName, List<ClientSubscription> clientSubscriptions, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(clientName, nameof(clientName));
            InternalContract.RequireNotNull(clientSubscriptions, nameof(clientSubscriptions));

            var relativeUrl = $"Clients/{WebUtility.UrlEncode(clientName)}/Subscriptions";
            await RestClient.PostNoResponseContentAsync(relativeUrl, clientSubscriptions, cancellationToken: cancellationToken);
        }
    }
}
