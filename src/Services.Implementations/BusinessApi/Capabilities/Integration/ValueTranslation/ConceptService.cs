using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.ValueTranslation
{
    /// <inheritdoc />
    public class ConceptService : IConceptService
    {
        private readonly ConceptsClient _conceptsClient;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ConceptService(ConceptsClient conceptsClient)
        {
            _conceptsClient = conceptsClient;
        }

        /// <inheritdoc />
        public Task<IEnumerable<IDictionary<string, string>>> GetAllInstancesAsync(string conceptName, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(conceptName, nameof(conceptName));
            return _conceptsClient.GetAllInstancesAsync(conceptName, cancellationToken);
        }
    }
}