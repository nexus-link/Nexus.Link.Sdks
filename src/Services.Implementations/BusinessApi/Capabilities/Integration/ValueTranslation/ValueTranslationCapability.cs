﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.KeyTranslator.Sdk;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation;
using ConceptValue = Nexus.Link.KeyTranslator.Sdk.ConceptValue;

namespace Nexus.Link.Services.Implementations.BusinessApi.Capabilities.Integration.ValueTranslation
{
    /// <inheritdoc />
    public class ValueTranslationCapability : IValueTranslationCapability
    {
        /// <inheritdoc />
        public ValueTranslationCapability(string serviceBaseUrl, ServiceClientCredentials serviceClientCredentials)
        {
            InternalContract.RequireNotNullOrWhiteSpace(serviceBaseUrl, nameof(serviceBaseUrl));
            InternalContract.RequireNotNull(serviceClientCredentials, nameof(serviceClientCredentials));
            var translatorClient = new TranslateClient(serviceBaseUrl, FulcrumApplication.Setup.Tenant, serviceClientCredentials);
            TranslatorService = new TranslatorService(translatorClient);
            var associationsClient = new AssociationsClient(serviceBaseUrl, FulcrumApplication.Setup.Tenant,
                serviceClientCredentials);
            AssociationService = new ValueAssociationsService(translatorClient, associationsClient);
        }

        /// <inheritdoc />
        public ITranslatorService TranslatorService { get; }

        /// <inheritdoc />
        public IValueAssociationsService AssociationService { get; }
    }

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

            FulcrumAssert.IsNotNull(valueOrLockId.IsLock);
            if (valueOrLockId.IsLock == null) return; // This will never be true. This line is here to satisfy automatic code checkers.
            if (!valueOrLockId.IsLock.Value)
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

    /// <summary>
    /// Methods for associating values with each other
    /// </summary>
    public interface IValueAssociationsService
    {
        /// <summary>
        /// Associate concept values.
        /// </summary>
        /// <param name="sourceConceptValuePath">The "old" concept value that we would like to associate to.</param>
        /// <param name="targetConceptValuePath">The "new" concept value that we would like to add as an association to <paramref name="sourceConceptValuePath"/>.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <remarks>Only use this associate method for items that already exists. If you du to parallelism face any risk of creating doublets then you should use
        /// <see cref="AssociateWithLockAsync"/> instead.</remarks>
        Task AssociateAsync(string sourceConceptValuePath, string targetConceptValuePath,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Translate a concept value to a target context. If no translation exists, a lock is returned.
        /// </summary>
        /// <param name="sourceConceptValuePath">The concept value that we would like to translate.</param>
        /// <param name="targetContextName">The name of the target context for the translation.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Either a translation or a lock id. The lock id is supposed to be used in <see cref=""/>.</returns>
        Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceConceptValuePath, string targetContextName,
            CancellationToken cancellationToken = default(CancellationToken));
        
        /// <summary>
        /// Translate a concept value to a target context. If no translation exists, a lock is returned.
        /// </summary>
        /// <param name="sourceConceptValuePath">The concept value that we would like to translate.</param>
        /// <param name="targetClientName">The name of the target client for the translation; the client whose context we will use as the target context.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Either a translation or a lock id. The lock id is supposed to be used in <see cref=""/>.</returns>
        Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceConceptValuePath, string targetClientName,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Associate one concept value path with another, using the lock that was obtained from either
        /// <see cref="TranslateToContextOrLockAsync"/> or <see cref="TranslateToClientOrLockAsync"/>.
        /// </summary>
        /// <param name="sourceConceptValuePath">The "old" concept value that we would like to associate to.</param>
        /// <param name="targetConceptValuePath">The "new" concept value that we would like to add as an association to <paramref name="sourceConceptValuePath"/>.</param>
        /// <param name="lockId">The lock that was created in <see cref="TranslateToContextOrLockAsync"/> or
        /// <see cref="TranslateToClientOrLockAsync"/>.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        Task AssociateWithLockAsync(string sourceConceptValuePath, string targetConceptValuePath, string lockId,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Associate one concept value path with another, using the lock that was obtained from either
        /// <see cref="TranslateToContextOrLockAsync"/> or <see cref="TranslateToClientOrLockAsync"/>.
        /// </summary>
        /// <param name="conceptValuePath">The concept value that the lock is for.</param>
        /// <param name="lockId">The lock that was created in <see cref="TranslateToContextOrLockAsync"/> or
        /// <see cref="TranslateToClientOrLockAsync"/>.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        Task ReleaseLockAsync(string conceptValuePath, string lockId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
