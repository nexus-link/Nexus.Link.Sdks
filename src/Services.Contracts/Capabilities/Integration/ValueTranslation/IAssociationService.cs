using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Platform.ValueTranslator;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation
{
    /// <summary>
    /// Methods for associating values with each other
    /// </summary>
    public interface IAssociationService
    {
        /// <summary>
        /// Associate concept values.
        /// </summary>
        /// <param name="sourceConceptValuePath">The "old" concept value that we would like to associate to.</param>
        /// <param name="targetConceptValuePaths">The "new" concept values that we would like to add as associations to <paramref name="sourceConceptValuePath"/>.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <remarks>Only use this associate method for items that already exists. If you du to parallelism face any risk of creating doublets then you should use
        /// <see cref="AssociateUsingLockAsync"/> instead.</remarks>
        Task AssociateAsync(string sourceConceptValuePath, string[] targetConceptValuePaths,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Translate a concept value to a target context. If no translation exists, a lock is returned.
        /// </summary>
        /// <param name="sourceConceptValuePath">The concept value that we would like to translate.</param>
        /// <param name="targetContextName">The name of the target context for the translation.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Either a translation or a lock id. The lock id is supposed to be used in <see cref="AssociateUsingLockAsync"/>.</returns>
        Task<ValueOrLockId> TranslateToContextOrLockAsync(string sourceConceptValuePath, string targetContextName,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Translate a concept value to a target context. If no translation exists, a lock is returned.
        /// </summary>
        /// <param name="sourceConceptValuePath">The concept value that we would like to translate.</param>
        /// <param name="targetClientName">The name of the target client for the translation; the client whose context we will use as the target context.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Either a translation or a lock id. The lock id is supposed to be used in <see cref=""/>.</returns>
        Task<ValueOrLockId> TranslateToClientOrLockAsync(string sourceConceptValuePath, string targetClientName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Associate one concept value path with another, using the lock that was obtained from either
        /// <see cref="TranslateToContextOrLockAsync"/> or <see cref="TranslateToClientOrLockAsync"/>.
        /// </summary>
        /// <param name="sourceConceptValuePath">The "old" concept value that we would like to associate to.</param>
        /// <param name="lockId">The lock that was created in <see cref="TranslateToContextOrLockAsync"/> or
        /// <see cref="TranslateToClientOrLockAsync"/>.</param>
        /// <param name="targetConceptValuePath">The "new" concept value that we would like to add as an association to <paramref name="sourceConceptValuePath"/>.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        Task AssociateUsingLockAsync(string sourceConceptValuePath, string lockId, string targetConceptValuePath,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Associate one concept value path with another, using the lock that was obtained from either
        /// <see cref="TranslateToContextOrLockAsync"/> or <see cref="TranslateToClientOrLockAsync"/>.
        /// </summary>
        /// <param name="conceptValuePath">The concept value that the lock is for.</param>
        /// <param name="lockId">The lock that was created in <see cref="TranslateToContextOrLockAsync"/> or
        /// <see cref="TranslateToClientOrLockAsync"/>.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        Task ReleaseLockAsync(string conceptValuePath, string lockId, CancellationToken cancellationToken = default);
    }
}