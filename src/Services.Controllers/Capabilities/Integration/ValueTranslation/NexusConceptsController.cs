using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.ValueTranslator;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;

namespace Nexus.Link.Services.Controllers.Capabilities.Integration.ValueTranslation
{
    /// <summary>
    /// Service implementation of <see cref="IConceptService"/>
    /// </summary>
    [ApiController]
    [Area("ValueTranslation")]
    [Route("api/Integration/v1/[area]/v1/Concepts")]
    public class NexusConceptsController : ControllerBase, IConceptService
    {
        /// <summary>
        /// The logic that we will use to implement the API.
        /// </summary>
        protected readonly IValueTranslationCapability Capability;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capability">The logic layer</param>
        public NexusConceptsController(IValueTranslationCapability capability)
        {
            Capability = capability;
        }

        /// <inheritdoc />
        [Route("{conceptName}/Instances")]
        [HttpGet]
        public Task<IEnumerable<IDictionary<string, string>>> GetAllInstancesAsync(string conceptName, CancellationToken cancellationToken = default(CancellationToken))
        {
            ServiceContract.RequireNotNullOrWhiteSpace(conceptName, nameof(conceptName));
            return Capability.ConceptService.GetAllInstancesAsync(conceptName, cancellationToken);
        }
    }
}
