using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;

namespace Nexus.Link.KeyTranslator.Sdk
{
    /// <summary>
    /// Translate a batch of values.
    /// </summary>
    public interface IBatchTranslate
    {
        /// <summary>
        /// The name of the source client, i.e. the client that created the values that should be translated.
        /// </summary>
        string SourceClientName { get; }

        /// <summary>
        /// The name of the target client, i.e. the client whose domain that we will translate the values to.
        /// </summary>
        string TargetClientName { get; }

        /// <summary>
        /// The service client that will do the actual translations.
        /// </summary>
        ITranslateClient TranslateClient { get; }

        /// <summary>
        /// Add another client based translation to the batch.
        /// </summary>
        /// <remarks>Assumes that <see cref="SourceClientName"/> and <see cref="TargetClientName"/> are set.</remarks>
        /// <param name="concept">The concept</param>
        /// <param name="sourceValue">The value that should be translated</param>
        /// <param name="action">The action that should be taken when the translation has been performed.</param>
        /// <returns>Will return "this", so that you can chain methods.</returns>
        BatchTranslate Add(string concept, string sourceValue, Action<string> action = null);

        /// <summary>
        /// Add another context based translation to the batch.
        /// </summary>
        /// <remarks>Ignores <see cref="SourceClientName"/> and <see cref="TargetClientName"/>.</remarks>
        /// <param name="concept">The concept</param>
        /// <param name="sourceContext">Source context</param>
        /// <param name="sourceValue">The value that should be translated</param>
        /// <param name="targetContext">Target context</param>
        /// <param name="action">The action that should be taken when the translation has been performed.</param>
        /// <returns>Will return "this", so that you can chain methods.</returns>
        BatchTranslate AddWithContexts(string concept, string sourceContext, string sourceValue, string targetContext, Action<string> action = null);

        /// <summary>
        /// Execute all translations.
        /// </summary>
        Task ExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a translation after the batch has been executed.
        /// </summary>
        /// <param name="concept">The concept.</param>
        /// <param name="sourceValue">The source value.</param>
        string this[string concept, string sourceValue] { get; }
    }
}