using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Health.Model;

namespace Nexus.Link.AsyncCaller.Sdk.Storage.Queue
{
    /// <summary>
    /// A generic interface for adding strings to a queue.
    /// </summary>
    public interface IQueue : IResourceHealth, IResourceHealth2
    {
        /// <summary>
        /// The name of the queue.
        /// </summary>
        string QueueName { get; }
        /// <summary>
        /// Connect to the named queue. Create it if it didn't exist.
        /// </summary>
        /// <param name="name">The name of the queue to connect to.</param>
        /// <returns>True if the queue needed to be created.</returns>
        bool MaybeCreateAndConnect(string name);

        /// <summary>
        /// Add a message to the queue.
        /// </summary>
        /// <param name="message">The message to add</param>
        /// <param name="timeSpanToWait">An optional time span to wait before the item can be taken from the queue.</param>
        Task AddMessageAsync(string message, TimeSpan? timeSpanToWait = null);

        /// <summary>
        /// "Reset"; clear the queue of all items.
        /// </summary>
        Task ClearAsync();
    }
}
