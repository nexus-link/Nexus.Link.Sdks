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
    public class AssociationsService : IAssociationsService
    {
        private readonly TranslateClient _translatorClient;
        private readonly IAssociationsClient _associationsClient;

        /// <inheritdoc />
        public AssociationsService(TranslateClient translatorClient, IAssociationsClient associationsClient)
        {
            _translatorClient = translatorClient;
            _associationsClient = associationsClient;
        }

        /// <inheritdoc />
        /// <remarks>This method is not complete yet. It does not support adding doublets. It will throw <see cref="FulcrumNotImplementedException"/> in those cases.
        /// To implement that, we will need a new service method in <see cref="IAssociationsClient"/>.</remarks>
        public async Task AssociateAsync(string sourceConceptValuePath, string [] targetConceptValuePaths,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNullOrWhiteSpace(sourceConceptValuePath, nameof(sourceConceptValuePath));
            InternalContract.RequireNotNull(targetConceptValuePaths, nameof(targetConceptValuePaths));
            var success = ConceptValue.TryParse(sourceConceptValuePath, out var sourceConceptValue);
            InternalContract.Require(success,
                $"Parameter {nameof(sourceConceptValuePath)} ({sourceConceptValuePath}) is not a concept value.");
            foreach (var targetConceptValuePath in targetConceptValuePaths)
            {
                InternalContract.RequireNotNullOrWhiteSpace(targetConceptValuePath, nameof(targetConceptValuePaths), $"The individual values of {nameof(targetConceptValuePaths)} must not be null or empty.");
                success = ConceptValue.TryParse(targetConceptValuePath, out var targetConceptValue);
                InternalContract.Require(success,
                    $"Parameter {nameof(targetConceptValuePaths)} contained at least one item ({targetConceptValuePath}) that is not a concept value.");
                InternalContract.Require(sourceConceptValue.ConceptName == targetConceptValue.ConceptName,
                    $"Parameter {nameof(targetConceptValuePaths)} contained at least one item ({targetConceptValuePath}) that has an unexpected concept name ({sourceConceptValue.ConceptName}) - should have {targetConceptValue.ConceptName}");
            }

            foreach (var targetConceptValuePath in targetConceptValuePaths)
            {
                var targetConceptValue = ConceptValue.Parse(targetConceptValuePath);

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

                await AssociateUsingLockAsync(sourceConceptValuePath, targetConceptValuePath, valueOrLockId.LockId,
                    cancellationToken);
            }
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
        public async Task AssociateUsingLockAsync(string sourceConceptValuePath, string lockId, string targetConceptValuePath,
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