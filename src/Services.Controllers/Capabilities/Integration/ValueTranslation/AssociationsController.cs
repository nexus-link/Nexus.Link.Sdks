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
    /// Service implementation of <see cref="IBusinessEventService"/>
    /// </summary>
    [ApiController]
    [Area("ValueTranslation")]
    [Route("api/Integration/v1/[area]/v1/Associations")]
    public class AssociationsController : ControllerBase, IAssociationsService
    {
        protected readonly IValueTranslationCapability Capability;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capability">The logic layer</param>
        public AssociationsController(IValueTranslationCapability capability)
        {
            Capability = capability;
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("{sourceId}")]
        public Task AssociateAsync(string sourceId, string[] targetIds,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ServiceContract.RequireNotNullOrWhiteSpace(sourceId, nameof(sourceId));
            ServiceContract.RequireNotNull(targetIds, nameof(targetIds));
            foreach (var targetConceptValuePath in targetIds)
            {
                InternalContract.RequireNotNullOrWhiteSpace(targetConceptValuePath, nameof(targetIds), $"The individual values of {nameof(targetIds)} must not be null or empty.");
            }

            return Capability.AssociationService.AssociateAsync(sourceId, targetIds,
                cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet]
        [Route("{sourceId}/Context/{targetId}/ValueOrLock")]
        public Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceId, string targetId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceId, nameof(sourceId));
            InternalContract.RequireNotNullOrWhiteSpace(targetId, nameof(targetId));

            return Capability.AssociationService.TranslateToContextOrLockAsync(sourceId, targetId,
                cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet]
        [Route("{sourceId}/Client/{targetId}/ValueOrLock")]
        public Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceId, string targetId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceId, nameof(sourceId));
            InternalContract.RequireNotNullOrWhiteSpace(targetId, nameof(targetId));

            return Capability.AssociationService.TranslateToClientOrLockAsync(sourceId, targetId,
                cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("{sourceId}/Locks/{lockId}")]
        public Task AssociateUsingLockAsync(string sourceId, string lockId, string targetId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceId, nameof(sourceId));
            InternalContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));
            InternalContract.RequireNotNullOrWhiteSpace(targetId, nameof(targetId));

            return Capability.AssociationService.AssociateUsingLockAsync(sourceId, lockId, targetId,
                cancellationToken);
        }

        /// <inheritdoc />
        [HttpDelete]
        [Route("{sourceId}/Locks/{lockId}")]
        public Task ReleaseLockAsync(string sourceId, string lockId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceId, nameof(sourceId));
            InternalContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));

            return Capability.AssociationService.ReleaseLockAsync(sourceId, lockId,
                cancellationToken);
        }
    }
}
