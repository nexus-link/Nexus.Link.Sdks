using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.KeyTranslator.Sdk
{
    /// <inheritdoc />
    public class ServiceHealth : IServiceHealth
    {

        private readonly IServiceMetasClient _translateClient;
        
        /// <inheritdoc />
        public ServiceHealth(IServiceMetasClient translateClient)
        {
            _translateClient = translateClient;
        }

        /// <inheritdoc />
        public async Task<HealthResponse> GetResourceHealthAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireValidated(tenant, nameof(tenant));
            try
            {
                var result = await _translateClient.ServiceHealthAsync(tenant);
                return result;
            }
            catch (Exception e)
            {
                return new HealthResponse
                {
                    Resource = "KeyTranslator",
                    Status = HealthResponse.StatusEnum.Error,
                    Message = e.Message
                };
            }
        }

    }
}
