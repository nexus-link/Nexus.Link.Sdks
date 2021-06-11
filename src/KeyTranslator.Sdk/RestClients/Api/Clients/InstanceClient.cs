using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Models;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Base;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk.RestClients.Api.Clients
{
    public class InstanceClient : BaseClient, IInstanceClient
    {
        public InstanceClient(string baseUri, Tenant tenant, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, tenant, authenticationCredentials)
        {
        }

        public async Task<Instance> GetOneAsync(Guid instanceId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Instances/{instanceId}";
            var result = await RestClient.GetAsync<Instance>(relativeUrl, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Instance> UpdateAsync(Guid instanceId, Instance instance, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Instances/{instanceId}";
            var result = await RestClient.PutAndReturnUpdatedObjectAsync(relativeUrl, instance, cancellationToken: cancellationToken);
            return result;
        }

        public async Task DeleteAsync(Guid instanceId, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"/Instances/{instanceId}";
            await RestClient.DeleteAsync(relativeUrl, cancellationToken: cancellationToken);
        }
    }
}
