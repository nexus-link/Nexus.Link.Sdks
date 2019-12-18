using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Platform.ValueTranslator;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.ValueTranslation
{
    /// <inheritdoc />
    public class ValueAssociationsService : IValueAssociationsService
    {
        private readonly TranslateClient _translatorClient;
        private readonly IAssociationsClient _associationsClient;

        /// <inheritdoc />
        public ValueAssociationsService(TranslateClient translatorClient, IAssociationsClient associationsClient)
        {
            _translatorClient = translatorClient;
            _associationsClient = associationsClient;
        }

        /// <inheritdoc />
        /// <remarks>This method is not complete yet. It does not support adding doublets. It will throw <see cref="FulcrumNotImplementedException"/> in those cases.
        /// To implement that, we will need a new service method in <see cref="IAssociationsClient"/>.</remarks>
        public async Task AssociateAsync(string sourceConceptValuePath, string targetConceptValuePath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            InternalContract.RequireNotNullOrWhiteSpace(targetConceptValuePath, nameof(targetConceptValuePath));
            var success = ConceptValue.TryParse(sourceConceptValuePath, out var sourceConceptValue);
            InternalContract.Require(success,
                $"Parameter {nameof(sourceConceptValuePath)} ({sourceConceptValuePath}) is not a concept value.");
            success = ConceptValue.TryParse(targetConceptValuePath, out var targetConceptValue);
            InternalContract.Require(success,
                $"Parameter {nameof(targetConceptValue)} ({targetConceptValue}) is not a concept value.");
            InternalContract.Require(sourceConceptValue.ConceptName == targetConceptValue.ConceptName,
                $"The two concept values must have the same concept. Had {sourceConceptValue.ConceptName} and {targetConceptValue.ConceptName}");

            ValueOrLockId valueOrLockId = null;
            string targetContextOrClient;
            if (targetConceptValue.ClientName == null)
            {
                valueOrLockId = await TranslateToContextOrLockAsync(sourceConceptValuePath,
                    targetConceptValue.ContextName, cancellationToken);
                targetContextOrClient = $"context {targetConceptValue.ContextName}";
            }
            else
            {
                valueOrLockId = await TranslateToClientOrLockAsync(sourceConceptValuePath,
                    targetConceptValue.ClientName, cancellationToken);
                targetContextOrClient = $"client {targetConceptValue.ClientName}";
            }
            if (!valueOrLockId.IsLock)
            {
                throw new FulcrumNotImplementedException(
                    $"Method {nameof(AssociateAsync)} does not yet support adding doublets and" +
                    $" there was already a translation from {sourceConceptValuePath} to {targetContextOrClient} ({valueOrLockId.Value}).");
            }

            await AssociateWithLockAsync(sourceConceptValuePath, targetConceptValuePath, valueOrLockId.LockId,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceConceptValuePath, string targetContextName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            InternalContract.RequireNotNullOrWhiteSpace(targetContextName, nameof(targetContextName));
            return _translatorClient.TranslateToContextOrLock2Async(sourceConceptValuePath, targetContextName,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceConceptValuePath, string targetClientName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            InternalContract.RequireNotNullOrWhiteSpace(targetClientName, nameof(targetClientName));
            return _translatorClient.TranslateToClientOrLock2Async(sourceConceptValuePath, targetClientName,
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task AssociateWithLockAsync(string sourceConceptValuePath, string targetConceptValuePath, string lockId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            InternalContract.RequireNotNullOrWhiteSpace(targetConceptValuePath, nameof(targetConceptValuePath));
            var success = ConceptValue.TryParse(sourceConceptValuePath, out var sourceConceptValue);
            InternalContract.Require(success,
                $"Parameter {nameof(sourceConceptValuePath)} ({sourceConceptValuePath}) is not a concept value.");
            success = ConceptValue.TryParse(targetConceptValuePath, out var targetConceptValue);
            InternalContract.Require(success,
                $"Parameter {nameof(targetConceptValue)} ({targetConceptValue}) is not a concept value.");
            InternalContract.Require(sourceConceptValue.ConceptName == targetConceptValue.ConceptName,
                $"The two concept values must have the same concept. Had {sourceConceptValue.ConceptName} and {targetConceptValue.ConceptName}");

            var association = new Association
            {
                LockId = lockId,
                SourcePath = sourceConceptValuePath,
                TargetContextName = targetConceptValue.ContextName,
                TargetClientName = targetConceptValue.ClientName,
                TargetValue = targetConceptValue.Value
            };
            await _associationsClient.CreateAsync(association);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(string conceptValuePath, string lockId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _translatorClient.ReleaseLock2Async(conceptValuePath, lockId, cancellationToken);
        }
    }
}