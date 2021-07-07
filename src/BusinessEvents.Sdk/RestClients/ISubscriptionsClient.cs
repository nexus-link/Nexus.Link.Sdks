using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;

namespace Nexus.Link.BusinessEvents.Sdk.RestClients
{
    internal interface ISubscriptionsClient
    {
        Task RegisterSubscriptions(string clientName, List<ClientSubscription> clientSubscriptions, CancellationToken cancellationToken = default);
    }
}
