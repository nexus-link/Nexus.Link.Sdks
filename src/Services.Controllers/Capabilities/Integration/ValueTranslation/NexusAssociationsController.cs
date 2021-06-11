using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.ValueTranslator;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;

namespace Nexus.Link.Services.Controllers.Capabilities.Integration.ValueTranslation
{
    /// <summary>
    /// Service implementation of <see cref="IAssociationService"/>
    /// </summary>
    [ApiController]
    [Area("ValueTranslation")]
    [Route("api/Integration/v1/[area]/v1/Associations")]
    public class NexusAssociationsController : ControllerBase, IAssociationService
    {
        protected readonly IValueTranslationCapability Capability;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capability">The logic layer</param>
        public NexusAssociationsController(IValueTranslationCapability capability)
        {
            Capability = capability;
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("{sourceConceptValuePath}")]
        public Task AssociateAsync(string sourceConceptValuePath, string[] targetConceptValuePaths,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            ServiceContract.RequireNotNull(targetConceptValuePaths, nameof(targetConceptValuePaths));
            foreach (var targetConceptValuePath in targetConceptValuePaths)
            {
                ServiceContract.RequireNotNullOrWhiteSpace(targetConceptValuePath, nameof(targetConceptValuePaths), $"The individual values of {nameof(targetConceptValuePaths)} must not be null or empty.");
            }

            return Capability.AssociationService.AssociateAsync(sourceConceptValuePath, targetConceptValuePaths,
                cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet]
        [Route("{sourceConceptValuePath}/Context/{targetContextName}/ValueOrLock")]
        public Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceConceptValuePath, string targetContextName,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            ServiceContract.RequireNotNullOrWhiteSpace(targetContextName, nameof(targetContextName));

            return Capability.AssociationService.TranslateToContextOrLockAsync(sourceConceptValuePath, targetContextName,
                cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet]
        [Route("{sourceConceptValuePath}/Client/{targetClientName}/ValueOrLock")]
        public Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceConceptValuePath, string targetClientName,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            ServiceContract.RequireNotNullOrWhiteSpace(targetClientName, nameof(targetClientName));

            return Capability.AssociationService.TranslateToClientOrLockAsync(sourceConceptValuePath, targetClientName,
                cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("{sourceConceptValuePath}/Locks/{lockId}")]
        public Task AssociateUsingLockAsync(string sourceConceptValuePath, string lockId, string targetConceptValuePath,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            ServiceContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));
            ServiceContract.RequireNotNullOrWhiteSpace(targetConceptValuePath, nameof(targetConceptValuePath));

            return Capability.AssociationService.AssociateUsingLockAsync(sourceConceptValuePath, lockId, targetConceptValuePath,
                cancellationToken);
        }

        /// <inheritdoc />
        [HttpDelete]
        [Route("{sourceConceptValuePath}/Locks/{lockId}")]
        public Task ReleaseLockAsync(string sourceConceptValuePath, string lockId,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            ServiceContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));

            return Capability.AssociationService.ReleaseLockAsync(sourceConceptValuePath, lockId,
                cancellationToken);
        }
    }
}
