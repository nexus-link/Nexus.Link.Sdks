using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.DataSyncEngine;

namespace Nexus.Link.DatasyncEngine.Sdk.RestClients
{
    public class SynchronizedEntityClient : BaseClient, ISynchronizedEntityClient
    {
        public SynchronizedEntityClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
            var isWellFormedUriString = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWellFormedUriString) throw new ArgumentException($"{nameof(baseUri)} must be a well formed uri");
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            if (authenticationCredentials == null) throw new ArgumentNullException(nameof(tenant));
        }

        /// <summary>
        /// Synchronize a main key with other keys.
        /// </summary>
        /// <remarks>This is useful when a business flow creates instances of the same entity in different system and wants to associate them in DSE.</remarks>
        public async Task SynchronizedEntityAssociatedAsync(KeyAssociations associations, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(associations, nameof(associations));

            const string relativeUrl = "Match/SynchronizedEntity/Associated";
            await RestClient.PostNoResponseContentAsync(relativeUrl, associations);
        }
    }
}
