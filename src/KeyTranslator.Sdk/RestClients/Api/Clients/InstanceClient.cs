using System;
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

        public async Task<Instance> GetOneAsync(Guid instanceId)
        {
            var relativeUrl = $"/Instances/{instanceId}";
            var result = await RestClient.GetAsync<Instance>(relativeUrl);
            return result;
        }

        public async Task<Instance> UpdateAsync(Guid instanceId, Instance instance)
        {
            var relativeUrl = $"/Instances/{instanceId}";
            var result = await RestClient.PutAndReturnUpdatedObjectAsync(relativeUrl, instance);
            return result;
        }

        public async Task DeleteAsync(Guid instanceId)
        {
            var relativeUrl = $"/Instances/{instanceId}";
            await RestClient.DeleteAsync(relativeUrl);
        }
    }
}
