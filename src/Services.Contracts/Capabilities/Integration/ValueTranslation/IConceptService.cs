using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Services.Contracts.Capabilities.Integration.ValueTranslation
{
    /// <summary>
    /// Methods for associating values with each other
    /// </summary>
    public interface IConceptService
    {
        /// <summary>
        /// Get all preferred instances for a concept, grouped as they are associated.
        /// </summary>
        /// <param name="conceptName">The concept that we want the instances for.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>An array, where each item is a key-value list of context and value for the instances that are associated.</returns>
        /// <example>For the concept "Gender", we might return
        /// [
        ///   {
        ///     "ms-crm": "200001",
        ///     "agresso": "M",
        ///     "en-us": "male"
        ///   },
        ///   {
        ///     "ms-crm": "200002",
        ///     "agresso": "F",
        ///     "en-us": "female"
        ///   },
        ///   {
        ///     "ms-crm": "200003",
        ///     "agresso": "U",
        ///     "en-us": "unknown"
        ///   }
        /// ]
        /// </example>
        Task<IEnumerable<IDictionary<string, string>>> GetAllInstancesAsync(string conceptName,
            CancellationToken cancellationToken = default);

    }
}